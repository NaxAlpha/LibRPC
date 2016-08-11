Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Forms

Module Server

	Private clients As New LinkedList(Of RPCHost)

	Sub Main()
		Task.Run(AddressOf Serve)
		Application.EnableVisualStyles()
		For i = 0 To 3
			Dim c As New Client
			c.Show()
		Next
		Application.Run()
	End Sub

	Sub Serve()
		Dim srv As New TcpListener(IPAddress.Any, 9990)
		srv.Start()
		While Not Console.KeyAvailable
			Dim sock = srv.AcceptSocket()
			Console.WriteLine("Client Added...")
			Dim strm = New NetworkStream(sock)
			Dim host As New RPCHost(strm)
			host.On(9901, New Func(Of String, String, Boolean)(AddressOf OnMessage))
			SyncLock clients
				clients.AddLast(host)
			End SyncLock
		End While
		srv.Stop()
	End Sub

	Private Function OnMessage(sender As String, msg As String) As Boolean
		Dim remers As New List(Of RPCHost)
		Console.ForegroundColor = ConsoleColor.Red
		Console.Write(sender + ": ")
		Console.ForegroundColor = ConsoleColor.Gray
		Console.Write(msg)
		For Each c In clients
			Task.Run(Sub()
						 Try
							 c.Call(Of Boolean)(9911, sender, msg)
						 Catch ex As Exception
							 SyncLock clients
								 clients.Remove(c)
								 Console.WriteLine("Client Removed...")
							 End SyncLock
						 End Try
					 End Sub)
		Next
		Return True
	End Function

End Module