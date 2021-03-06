﻿Imports System.Linq.Expressions
Imports System.Reflection
Imports System.IO

Namespace Basic

	''' <summary>
	''' Represents RPC Engine to perform actions
	''' </summary>
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

		''' <summary>
		''' Occures when Background worker is lost due to some error like stream closed etc.
		''' </summary>
		Public Event OnError As EventHandler(Of ExceptionEventArgs)

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
				Dim dat = Array.CreateInstance(type.GetElementType(), len)
				For i = 0 To len - 1
					dat.SetValue(Deserialize(typ), i)
				Next
				Return dat
			Else
				Throw New SerializationException("Cannot deserialize: " + type.FullName)
			End If
		End Function

		''' <summary>
		''' Creates new instance of RPCEngine
		''' </summary>
		''' <param name="stream">The stream on which RPC calls will be sent and received</param>
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
				RaiseEvent OnError(Me, New ExceptionEventArgs(task.Exception))
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

					Dim method = code.GetMethodInfo()

					Dim paramTypes = method.GetParameters().
					Select(Function(p) p.ParameterType).ToArray()

					Dim args = New Object(paramTypes.Length - 1) {}
					For i = 0 To args.Length - 1
						args(i) = Deserialize(paramTypes(i))
					Next

					Dim out As Object

					If method.ReturnType IsNot GetType(Void) Then
						out = code.DynamicInvoke(args)
					Else
						out = True
					End If

					SyncLock Output
						Output.Write(CShort(0))
						Output.Write(reqId)
						Serialize(out)
					End SyncLock

				End If

			End While

		End Sub

		''' <summary>
		''' Calls a remote host by given OpCode
		''' </summary>
		''' <typeparam name="T">Return type</typeparam>
		''' <param name="opCode">OpCode of target function</param>
		''' <param name="args">Arguments of target function</param>
		''' <returns></returns>
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

		''' <summary>
		''' Calls function with void return type
		''' </summary>
		''' <param name="opCode"></param>
		''' <param name="args"></param>
		Public Sub CallVoid(opCode As Short, ParamArray args() As Object)
			Me.Call(Of Boolean)(opCode, args)
		End Sub


		''' <summary>
		''' Performs a remote procedure call asynchronously
		''' </summary>
		''' <typeparam name="T"></typeparam>
		''' <param name="opCode"></param>
		''' <param name="args"></param>
		''' <returns></returns>
		Public Async Function CallAsync(Of T)(opCode As Short, ParamArray args() As Object) As Task(Of T)
			Return Await Task.Run(Function() Me.Call(Of T)(opCode, args))
		End Function

		''' <summary>
		''' Performs a remote procedure call asynchronously
		''' </summary>
		''' <param name="opCode"></param>
		''' <param name="args"></param>
		''' <returns></returns>
		Public Async Function CallAsync(opCode As Short, ParamArray args() As Object) As Task
			Await Task.Run(Sub() Me.CallVoid(opCode, args))
		End Function

#If DESKTOP Then

		''' <summary>
		''' Creates a Proxy for calling remote functions using OOP manner
		''' </summary>
		''' <typeparam name="TInterface">Interface type which contains functions with attribute RPCAttribute</typeparam>
		''' <returns></returns>
		Public Function CreateProxy(Of TInterface)() As TInterface
			Return MetaHost.BuildCaller(Of TInterface)(Me)
		End Function

#End If

		''' <summary>
		''' Adds specific handler for specific opcode
		''' </summary>
		''' <param name="opCode"></param>
		''' <param name="handler"></param>
		Public Sub [On](opCode As Short, handler As [Delegate])
			If Targets.ContainsKey(opCode) Then
				Targets(opCode) = handler
			Else
				Targets.Add(opCode, handler)
			End If
		End Sub

		''' <summary>
		''' An object which contains functions with RPCAttribute which are to be called when specific OpCode is received
		''' </summary>
		''' <param name="context"></param>
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
			If Not m.IsPublic Then Return False
			If m.IsGenericMethod Then Return False
			If m.GetCustomAttribute(Of RPCAttribute)(True) Is Nothing Then Return False
			Return True
		End Function

		''' <summary>
		''' Closes base stream and unmanaged resources
		''' </summary>
		Public Sub Close() Implements IDisposable.Dispose
			Output.Dispose()
			Input.Dispose()
			Base.Dispose()
		End Sub

	End Class

End Namespace