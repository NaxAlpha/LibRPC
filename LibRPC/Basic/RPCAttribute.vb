Namespace Basic

	''' <summary>
	''' Specifies RPC function OpCode
	''' </summary>
	<AttributeUsage(AttributeTargets.Method, Inherited:=True, AllowMultiple:=False)>
	Public Class RPCAttribute
		Inherits Attribute

		''' <summary>
		''' Gets the OpCode related to RPC function
		''' </summary>
		''' <returns></returns>
		Public ReadOnly Property OpCode As Short

		''' <summary>
		''' Creates instance of RPCAttribute
		''' </summary>
		''' <param name="opCode"></param>
		Public Sub New(opCode As Short)
			Me.OpCode = opCode
		End Sub
	End Class


End Namespace