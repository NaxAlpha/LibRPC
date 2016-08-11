Imports System.IO
Imports System.Linq.Expressions
Imports System.Reflection

Public Class RPCHost
	Implements IDisposable

	Private Rnd As Random
	Private Base As Stream
	Private Input As BinaryReader
	Private Output As BinaryWriter
	Private Targets As New Dictionary(Of Short, [Delegate])
	Private Requests As New Dictionary(Of Short, Type)
	Private Responses As New Dictionary(Of Short, Object)
	Private Primitives As New List(Of Type) From {GetType(Boolean), GetType(Char),
		GetType(Byte), GetType(Short), GetType(Integer), GetType(Long),
		GetType(SByte), GetType(UShort), GetType(UInteger), GetType(ULong),
		GetType(Single), GetType(Double), GetType(Decimal), GetType(String)}

	Public Event OnError As EventHandler(Of ExceptionEventHandler)

	Private Sub Serialize(obj As Object)
		Dim typ = obj.GetType()
		If Primitives.Contains(typ) Then
			Utils.SerializeBase(Output, obj)
		ElseIf typ.IsArray AndAlso Primitives.Contains(typ.GetElementType()) Then
			Dim x As Array = DirectCast(obj, Array)
			Output.Write(x.Length)
			For Each y In x
				Serialize(y)
			Next
		Else
			Throw New RPCHostException("Cannot serialize type: " + typ.FullName)
		End If
	End Sub
	Private Function Deserialize(type As Type) As Object
		If Primitives.Contains(type) Then
			Return Utils.DeserializeBase(Input, type)
		ElseIf type.IsArray AndAlso Primitives.Contains(type.GetElementType()) Then
			Dim typ = type.GetElementType()
			Dim len = Input.ReadInt32()
			Dim dat(len - 1) As Object
			For i = 0 To len - 1
				dat(i) = Deserialize(typ)
			Next
			Return dat
		Else
			Throw New SerializationException("Cannot deserialize: " + type.FullName)
		End If
	End Function

	Public Sub New(stream As Stream)

		'Validation
		If stream Is Nothing Then
			Throw New RPCHostException("Stream cannot be null", New NullReferenceException())
		End If
		If Not (stream.CanRead And stream.CanWrite) Then
			Throw New RPCHostException("Stream must be readable and writeable")
		End If

		Rnd = New Random()
		Base = stream
		Input = New BinaryReader(Base)
		Output = New BinaryWriter(Base)
		Utils.Async(AddressOf HandleWorker)
		LoadContext(Me)
	End Sub

	Private Sub HandleWorker()
		Dim task = Utils.Async(AddressOf Worker)
		task.Wait()
		If task.Exception IsNot Nothing Then
			RaiseEvent OnError(Me, New ExceptionEventHandler(task.Exception))
		End If
	End Sub

	Private Sub Worker()
		While True

			Dim opCode = Input.ReadInt16()
			Dim reqId = Input.ReadInt16()

			If opCode = 0 Then
				Utils.WaitFor(Function() Requests.ContainsKey(reqId))
				Dim type = Requests(reqId)
				Utils.Locked(Requests, Sub() Requests.Remove(reqId))
				Dim out = Deserialize(type)
				Utils.Locked(Responses, Sub() Responses.Add(reqId, out))
			Else
				Dim code = Targets(opCode)
				If code Is Nothing Then
					Throw New RPCHostException("Opcode Handler not found: " + opCode.ToString())
				End If

				Dim paramTypes = code.GetMethodInfo().GetParameters().
					Select(Function(p) p.ParameterType).ToArray()

				Dim args(paramTypes.Length - 1) As Object
				For i = 0 To args.Length - 1
					args(i) = Deserialize(paramTypes(i))
				Next

				Dim out = code.DynamicInvoke(args.ToArray())

				SyncLock Output
					Output.Write(CShort(0))
					Output.Write(reqId)
					Serialize(out)
				End SyncLock

			End If

		End While

	End Sub

	Public Function [Call](Of T)(opCode As Short, ParamArray args() As Object) As T
		Dim reqId = CShort(Rnd.Next(0, Short.MaxValue))
		Utils.Locked(Requests, Sub() Requests.Add(reqId, GetType(T)))

		SyncLock Output
			Output.Write(opCode)
			Output.Write(reqId)

			For Each arg In args
				Serialize(arg)
			Next
		End SyncLock

		Utils.WaitFor(Function() Responses.ContainsKey(reqId))
		Dim out = Responses(reqId)
		Utils.Locked(Responses, Sub() Responses.Remove(reqId))

		Return DirectCast(out, T)
	End Function

#If DESKTOP Then

	Public Function CreateCaller(Of T)() As T
		Return MetaHost.BuildCaller(Of T)(Me)
	End Function

#End If

	Public Sub [On](opCode As Short, handler As [Delegate])
		If Targets.ContainsKey(opCode) Then
			Targets(opCode) = handler
		Else
			Targets.Add(opCode, handler)
		End If
	End Sub

	Public Sub LoadContext(context As Object)
		Dim mx = context.GetType().GetRuntimeMethods().Where(AddressOf ValidateMethod)
		For Each method In mx
			Dim opc = method.GetCustomAttribute(Of RPCAttribute)().OpCode
			Dim typ = method.GetParameters().Select(Function(p) p.ParameterType)
			Dim tyx = typ.ToList()
			tyx.Add(method.ReturnType)
			Dim del = method.CreateDelegate(Expression.GetFuncType(tyx.ToArray()), context)
			Me.On(opc, del)
		Next
	End Sub

	Friend Shared Function ValidateMethod(m As MethodInfo) As Boolean
		If m.ReturnType Is GetType(Void) Then Return False
		If Not m.IsPublic Then Return False
		If m.IsGenericMethod Then Return False
		If m.GetCustomAttribute(Of RPCAttribute)(True) Is Nothing Then Return False
		Return True
	End Function

	Public Sub Close() Implements IDisposable.Dispose
		Output.Dispose()
		Input.Dispose()
		Base.Dispose()
	End Sub

End Class

