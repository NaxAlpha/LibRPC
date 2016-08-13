''' <summary>
''' Represents exception occured by MetaHost while generating proxy
''' </summary>
Public Class MetaHostException
	Inherits Exception

	''' <summary>
	''' Creates instance of MetaHost Exception
	''' </summary>
	''' <param name="message"></param>
	''' <param name="inner"></param>
	Public Sub New(message As String, Optional inner As Exception = Nothing)
		MyBase.New(message, inner)
	End Sub

End Class
