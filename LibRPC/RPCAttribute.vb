<AttributeUsage(AttributeTargets.Method, Inherited:=True, AllowMultiple:=False)>
Public Class RPCAttribute
	Inherits Attribute
	Public ReadOnly Property OpCode As Short
	Public Sub New(opCode As Short)
		Me.OpCode = opCode
	End Sub
End Class
