Public Class ExceptionEventHandler
	Inherits EventArgs

	Public ReadOnly Property Exception As Exception
	Public Sub New(exception As Exception)
		Me.Exception = exception
	End Sub

End Class
