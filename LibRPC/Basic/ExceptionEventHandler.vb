Namespace Basic

	''' <summary>
	''' Represents Event handler argument for exceptions
	''' </summary>
	Public Class ExceptionEventArgs
		Inherits EventArgs

		''' <summary>
		''' Get the Exception occured
		''' </summary>
		''' <returns></returns>
		Public ReadOnly Property Exception As Exception

		''' <summary>
		''' Creates new instance of ExceptionEventArgs
		''' </summary>
		''' <param name="exception"></param>
		Public Sub New(exception As Exception)
			Me.Exception = exception
		End Sub

	End Class

End Namespace