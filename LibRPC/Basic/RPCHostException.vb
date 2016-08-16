Namespace Basic

	''' <summary>
	''' Represents exception thrown by RPCHost
	''' </summary>
	Public Class RPCHostException
		Inherits Exception

		''' <summary>
		''' Creates instance of RPCHostException
		''' </summary>
		''' <param name="message"></param>
		''' <param name="inner"></param>
		Public Sub New(message As String, Optional inner As Exception = Nothing)
			MyBase.New(message, inner)
		End Sub


	End Class


End Namespace