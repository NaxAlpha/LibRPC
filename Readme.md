## LibRPC

[![Join the chat at https://gitter.im/NaxAlpha/LibRPC](https://badges.gitter.im/NaxAlpha/LibRPC.svg)](https://gitter.im/NaxAlpha/LibRPC?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

LibRPC is very lightweight library for both .Net Framework and .Net Portable.
It provides stream based RPC Framework whick makes it very light and extensible.
It basically works on server/client and request/respond architucture.
Checkout Examples for basic usage

### Example

LibRPC uses stream based communication so you can use it in TCP, Pipes and any
other stream based protocol. Following is basic server-client example where
we simple call server to add two numbers:

#### Basic Server

```vb.net
	Private Sub Server()
		Dim tcp As New TcpListener(IPAddress.Loopback, 11221)
		tcp.Start()
		While True
			Dim sock = tcp.AcceptTcpClient()
			Dim host = New RPCHost(sock.GetStream())
			host.On(6644, New Func(Of Integer, Integer, Integer)(AddressOf OnAdd))
		End While
	End Sub

	Private Function OnAdd(x As Integer, y As Integer) As Integer
		Return x + y
	End Function
```

### Basic Client

```vb.net
	Private Sub Client()
		Dim tcp As New TcpClient("localhost", 11221)
		Dim host As New RPCHost(tcp.GetStream())
		Console.WriteLine(host.Call(Of Integer)(6644, 5, 9))
	End Sub
```
