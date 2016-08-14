Imports System.IO
Imports System.IO.Pipes
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class TcpTests

	Private Shared server As TcpListener

	Shared Sub New()
		server = New TcpListener(IPAddress.Loopback, 11221)
		server.Start()
		Task.Run(AddressOf DoWork)
	End Sub

	Private Shared Sub DoWork()
		While True
			Dim client = server.AcceptTcpClient()
			Dim host = New RPCHost(client.GetStream())
			host.On(6644, New Func(Of Long, Integer, Long)(AddressOf OnAdd))
			host.On(6645, New Func(Of String, String)(AddressOf OnEcho))
			host.On(6646, New Action(Of String)(AddressOf OnVoid))
		End While
	End Sub

	Private Shared Function OnAdd(x As Long, y As Integer) As Long
		Return x + y
	End Function

	Private Shared Function OnEcho(msg As String) As String
		Return msg
	End Function

	Private Shared Sub OnVoid(msg As String)
		If msg <> "Hello World" Then
			Throw New Exception("Could not match message")
		End If
	End Sub

	<TestMethod()>
	Public Sub CallTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			If host.Call(Of Long)(6644, CLng(5), (100)) <> 105 Then
				Throw New Exception("Invalid call results")
			End If
		End Using
	End Sub

	<TestMethod>
	Public Sub EchoTests()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			If host.Call(Of String)(6645, "Hello World") <> "Hello World" Then
				Throw New Exception("Invalid echo results")
			End If
		End Using
	End Sub

	<TestMethod>
	Public Sub VoidTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			host.Call(6646, "Hello World")
		End Using
	End Sub

End Class