Public Class RPCHostException
	Inherits Exception

	Public Sub New(message As String, Optional inner As Exception = Nothing)
		MyBase.New(message, inner)
	End Sub


End Class
