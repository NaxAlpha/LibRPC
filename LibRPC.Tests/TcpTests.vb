Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports LibRPC.Basic

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
			host.On(6647, New Func(Of Integer(), Integer)(AddressOf OnAddX))
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

	Private Shared Function OnAddX(x As Integer()) As Integer
		Return x.Sum()
	End Function

	<TestMethod()>
	Public Sub CallTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			Thread.Sleep(100)
			If host.Call(Of Long)(6644, CLng(5), (100)) <> 105 Then
				Throw New Exception("Invalid call results")
			End If
		End Using
	End Sub

	<TestMethod>
	Public Sub EchoTests()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			Thread.Sleep(100)
			If host.Call(Of String)(6645, "Hello World") <> "Hello World" Then
				Throw New Exception("Invalid echo results")
			End If
		End Using
	End Sub

	<TestMethod>
	Public Sub VoidTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			Thread.Sleep(100)
			host.CallVoid(6646, "Hello World")
		End Using
	End Sub

	<TestMethod>
	Public Sub MetaTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			Thread.Sleep(100)
			Dim hello = host.CreateProxy(Of IHello)()
			If hello.Add(1, 7) <> 8 Then
				Throw New Exception("Not Working add")
			End If
			If hello.Echo("Hello") <> "Hello" Then
				Throw New Exception("Not working echo")
			End If
			hello.Print("Hello World")
		End Using
	End Sub

	<TestMethod>
	Public Sub ArrayTest()
		Using tcp As New TcpClient("localhost", 11221),
				host = New RPCHost(tcp.GetStream())
			Thread.Sleep(100)
			host.Call(Of Integer)(6647, New Integer() {1, 5, 6})
		End Using
	End Sub

End Class
Public Interface IHello
	<RPC(6644)>
	Function Add(x As Long, y As Integer) As Long

	<RPC(6645)>
	Function Echo(msg As String) As String

	<RPC(6646)>
	Sub Print(msg As String)

End Interface