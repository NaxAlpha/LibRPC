Imports System.IO

Public Class DuelStream
	Inherits Stream

	Private writer As Stream
	Private reader As Stream

	Public Sub New(writer As Stream, reader As Stream)
		Me.writer = writer
		Me.reader = reader
	End Sub

	Public Overrides ReadOnly Property CanRead As Boolean
		Get
			Return reader.CanRead
		End Get
	End Property

	Public Overrides ReadOnly Property CanSeek As Boolean
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Overrides ReadOnly Property CanWrite As Boolean
		Get
			Return writer.CanWrite
		End Get
	End Property

	Public Overrides ReadOnly Property Length As Long
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Overrides Property Position As Long
		Get
			Throw New NotImplementedException()
		End Get
		Set(value As Long)
			Throw New NotImplementedException()
		End Set
	End Property

	Public Overrides Sub Flush()
		writer.Flush()
	End Sub

	Public Overrides Sub SetLength(value As Long)
		Throw New NotImplementedException()
	End Sub

	Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
		writer.Write(buffer, offset, count)
	End Sub

	Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
		Return reader.Read(buffer, offset, count)
	End Function

	Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
		Throw New NotImplementedException()
	End Function
End Class
