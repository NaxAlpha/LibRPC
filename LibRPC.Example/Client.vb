Imports System.Net.Sockets
Imports System.Windows.Forms

Public Class Client

	Private host As RPCHost

	Private Sub Ref()
		host?.Close()
		Dim tcp As New TcpClient("localhost", 9990)
		host = New RPCHost(tcp.GetStream())
		host.On(9911, New Func(Of String, String, Boolean)(AddressOf OnMessage))
	End Sub

	Private Sub Recon()
		Dim success = False
		While Not success
			Try
				Ref()
				success = True
			Catch ex As Exception
			End Try
		End While
	End Sub

	Private Sub Send()
		Try
			host.Call(Of Boolean)(9901, Text, txt.Text + vbNewLine)
		Catch ex As Exception
			Recon()
		End Try
		Invoke(Sub() txt.Text = "")
	End Sub

	Private Async Sub DoAsync(act As Action)
		Me.Enabled = False
		Await Task.Run(act)
		Me.Enabled = True
	End Sub

	Private Function OnMessage(sender As String, msg As String) As Boolean
		Try
			Me.Invoke(Sub()
						  txtMsg.DocumentText = "<b>" + sender + "</b>: " + msg + "<br/>" + txtMsg.DocumentText
					  End Sub)
		Catch ex As Exception
			host.Close()
		End Try
		Return True
	End Function

	Private Sub Client_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		DoAsync(AddressOf Recon)
	End Sub

	Private Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
		DoAsync(AddressOf Send)
	End Sub

	Private Sub txt_KeyDown(sender As Object, e As KeyEventArgs) Handles txt.KeyDown
		If e.KeyCode = Keys.Enter Then
			DoAsync(AddressOf Send)
		End If
	End Sub

End Class