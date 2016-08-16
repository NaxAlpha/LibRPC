Imports System.Reflection
Imports System.Reflection.Emit
Namespace Basic

#If DESKTOP Then


	Friend Class MetaHost

		Private Shared assemblyBuilder As AssemblyBuilder
		Private Shared moduleBuilder As ModuleBuilder
		Private Shared callMethod As MethodInfo
		Private Shared callVoidMethod As MethodInfo
		Private Shared typeCache As New Dictionary(Of Type, Type)

		Shared Sub New()
			Dim asmName As New AssemblyName("LibRPC.Meta")
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave)
			moduleBuilder = assemblyBuilder.DefineDynamicModule("LibRPC.Module", "LibRPC.dll")
			callMethod = GetType(RPCHost).GetMethod("Call")
			callVoidMethod = GetType(RPCHost).GetMethod("CallVoid")
		End Sub

		Public Shared Function BuildCaller(Of T)(host As RPCHost) As T
			Dim interfaceType = GetType(T)

			If Not interfaceType.IsInterface Then
				Throw New MetaHostException("Type must be interface: " + interfaceType.FullName)
			End If

			If Not typeCache.ContainsKey(interfaceType) Then

				Dim objType = moduleBuilder.DefineType(interfaceType.Name + "Implm", TypeAttributes.Class)
				Dim ownerField = AddFieldAndCttr(objType)

				objType.AddInterfaceImplementation(interfaceType)
				Dim methods = interfaceType.GetRuntimeMethods().Where(Function(m) RPCHost.ValidateMethod(m))

				For Each method In methods
					ImplementMethod(objType, method, ownerField)
				Next

				Dim outputType = objType.CreateType()
				typeCache.Add(interfaceType, outputType)
			End If

			Dim targetType = typeCache(interfaceType)

			Return DirectCast(Activator.CreateInstance(targetType, host), T)

		End Function

		Private Shared Function AddFieldAndCttr(type As TypeBuilder) As FieldInfo

			Dim field = type.DefineField("owner", GetType(RPCHost), FieldAttributes.InitOnly)
			Dim cttr = Sigil.NonGeneric.Emit.BuildConstructor({GetType(RPCHost)}, type, MethodAttributes.SpecialName Or MethodAttributes.RTSpecialName Or MethodAttributes.Public, CallingConventions.HasThis)

			cttr.Nop()
			cttr.LoadArgument(0)
			cttr.LoadArgument(1)
			cttr.StoreField(field)
			cttr.Return()

			cttr.CreateConstructor()
			Return field
		End Function

		Private Shared Sub ImplementMethod(type As TypeBuilder, method As MethodInfo, owner As FieldInfo)
			Dim id = method.GetCustomAttribute(Of RPCAttribute)().OpCode

			Dim params = method.GetParameters().Select(Function(p) p.ParameterType).ToArray()
			Dim mx = Sigil.NonGeneric.Emit.BuildInstanceMethod(method.ReturnType, params, type, method.Name, MethodAttributes.Public Or MethodAttributes.Final Or MethodAttributes.NewSlot Or MethodAttributes.Virtual Or MethodAttributes.HideBySig)

			mx.Nop()

			mx.LoadArgument(0)
			mx.LoadField(owner)
			mx.LoadConstant(id)

			mx.LoadConstant(params.Length)
			mx.NewArray(Of Object)()

			For i = 0 To params.Length - 1
				mx.Duplicate()
				mx.LoadConstant(i)
				mx.LoadArgument(i + 1)
				If Not params(i).IsClass Then
					mx.Box(params(i))
				End If
				mx.AsShorthand().Stelem(Of Object)()
			Next

			If method.ReturnType Is GetType(Void) Then
				mx.CallVirtual(callVoidMethod)
			Else
				Dim callee = callMethod.MakeGenericMethod(method.ReturnType)
				mx.CallVirtual(callee)
			End If

			mx.Return()

			type.DefineMethodOverride(mx.CreateMethod(), method)
		End Sub

		Friend Shared Sub Save()
			assemblyBuilder.Save("LibRPC.dll")
		End Sub

	End Class


#End If

End Namespace