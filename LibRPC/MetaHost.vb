

#If DESKTOP Then

Imports System.Reflection
Imports System.Reflection.Emit

Friend Class MetaHost

	Private Shared assemblyBuilder As AssemblyBuilder
	Private Shared moduleBuilder As ModuleBuilder
	Private Shared hostMethod As MethodInfo
	Private Shared typeCache As New Dictionary(Of Type, Type)

	Shared Sub New()
		Dim asmName As New AssemblyName("LibRPC.Meta")
		assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave)
		moduleBuilder = assemblyBuilder.DefineDynamicModule("LibRPC.Module", "LibRPC.dll")
		hostMethod = GetType(RPCHost).GetMethod("Call")
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
		Dim cttr = type.DefineConstructor(MethodAttributes.SpecialName Or MethodAttributes.RTSpecialName Or MethodAttributes.Public, CallingConventions.ExplicitThis, {GetType(RPCHost)})

		Dim il = cttr.GetILGenerator()
		il.Emit(OpCodes.Nop)
		il.Emit(OpCodes.Ldarg_0)
		il.Emit(OpCodes.Ldarg_1)
		il.Emit(OpCodes.Stfld, field)
		il.Emit(OpCodes.Ret)

		Return field
	End Function

	Private Shared Sub ImplementMethod(type As TypeBuilder, method As MethodInfo, owner As FieldInfo)

		Dim id = method.GetCustomAttribute(Of RPCAttribute)().OpCode

		Dim params = method.GetParameters().Select(Function(p) p.ParameterType).ToArray()
		Dim mx = type.DefineMethod(method.Name, MethodAttributes.Public Or MethodAttributes.Virtual Or MethodAttributes.HideBySig, CallingConventions.ExplicitThis, method.ReturnType, params)
		type.DefineMethodOverride(mx, method)

		Dim il = mx.GetILGenerator()
		il.Emit(OpCodes.Nop)

		il.Emit(OpCodes.Ldarg_0)
		il.Emit(OpCodes.Ldfld, owner)
		il.Emit(OpCodes.Ldc_I4, CInt(id))

		il.Emit(OpCodes.Ldc_I4, params.Length)
		il.Emit(OpCodes.Newarr, GetType(Object))

		For i = 0 To params.Length - 1
			il.Emit(OpCodes.Dup)
			il.Emit(OpCodes.Ldc_I4, i)
			il.Emit(OpCodes.Ldarg, (i + 1))
			If Not params(i).IsClass Then
				il.Emit(OpCodes.Box, params(i))
			End If
			il.Emit(OpCodes.Stelem_Ref)
		Next

		Dim callee = hostMethod.MakeGenericMethod(method.ReturnType)
		il.Emit(OpCodes.Callvirt, callee)

		il.Emit(OpCodes.Ret)
	End Sub

	Friend Shared Sub Save()
		assemblyBuilder.Save("LibRPC.dll")
	End Sub

End Class


#End If