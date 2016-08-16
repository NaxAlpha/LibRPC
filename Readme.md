## LibRPC 

[![Join the chat at https://gitter.im/NaxAlpha/LibRPC ](https://badges.gitter.im/NaxAlpha/LibRPC.svg)](https://gitter.im/NaxAlpha/LibRPC?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Continuous Build at https://ci.appveyor.com/project/NaxAlpha/librpc/ ](https://img.shields.io/appveyor/ci/NaxAlpha/librpc.svg)](https://ci.appveyor.com/project/NaxAlpha/librpc/)
[![Download Nuget package at https://www.nuget.org/packages/LibRPC/ ](https://img.shields.io/nuget/v/LibRPC.svg)](https://www.nuget.org/packages/LibRPC/)

LibRPC is very lightweight, realtime and high performance library for both .Net Framework and .Net Portable.
It provides stream based RPC Framework whick makes it very light and extensible.
It basically works on server/client and request/respond architucture.
Checkout Examples for basic usage

### Example

LibRPC uses stream based communication so you can use it in TCP, Pipes and any
other stream based protocol. Following is basic server-client example where
we simple call server to add two numbers `Imports LibRPC.Basic`:

#### Basic Server (VB.Net)

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

#### Basic Client (VB.Net)

```vb.net
	Private Sub Client()
		Dim tcp As New TcpClient("localhost", 11221)
		Dim host As New RPCHost(tcp.GetStream())
		Console.WriteLine(host.Call(Of Integer)(6644, 5, 9)) '14
	End Sub
```

---

Following is C# `using LibRPC.Basic;`:

#### Basic Server (C#.Net)

```csharp
	private void Server()
	{
		TcpListener tcp = new TcpListener(IPAddress.Loopback, 11221);
		tcp.Start();
		while (true) {
			dynamic sock = tcp.AcceptTcpClient();
			dynamic host = new RPCHost(sock.GetStream());
			host.On(6644, new Func<int, int, int>(OnAdd));
		}
	}

	private int OnAdd(int x, int y)
	{
		return x + y;
	}
```

#### Basic Client (C#.Net)

```csharp
	private void Client()
	{
		TcpClient tcp = new TcpClient("localhost", 11221);
		RPCHost host = new RPCHost(tcp.GetStream());
		Console.WriteLine(host.Call<int>(6644, 5, 9)); //14
	}
```