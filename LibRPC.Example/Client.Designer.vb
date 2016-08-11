<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Client
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.txtMsg = New System.Windows.Forms.WebBrowser()
		Me.btnSend = New System.Windows.Forms.Button()
		Me.txt = New System.Windows.Forms.TextBox()
		Me.SuspendLayout()
		'
		'txtMsg
		'
		Me.txtMsg.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.txtMsg.IsWebBrowserContextMenuEnabled = False
		Me.txtMsg.Location = New System.Drawing.Point(12, 12)
		Me.txtMsg.MinimumSize = New System.Drawing.Size(20, 20)
		Me.txtMsg.Name = "txtMsg"
		Me.txtMsg.ScrollBarsEnabled = False
		Me.txtMsg.Size = New System.Drawing.Size(531, 373)
		Me.txtMsg.TabIndex = 6
		'
		'btnSend
		'
		Me.btnSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnSend.Location = New System.Drawing.Point(461, 391)
		Me.btnSend.Name = "btnSend"
		Me.btnSend.Size = New System.Drawing.Size(82, 20)
		Me.btnSend.TabIndex = 5
		Me.btnSend.Text = "Send"
		Me.btnSend.UseVisualStyleBackColor = True
		'
		'txt
		'
		Me.txt.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.txt.Location = New System.Drawing.Point(12, 391)
		Me.txt.Name = "txt"
		Me.txt.Size = New System.Drawing.Size(443, 20)
		Me.txt.TabIndex = 4
		'
		'Client
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(555, 423)
		Me.Controls.Add(Me.txtMsg)
		Me.Controls.Add(Me.btnSend)
		Me.Controls.Add(Me.txt)
		Me.Name = "Client"
		Me.Text = "Client"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents txtMsg As System.Windows.Forms.WebBrowser
	Friend WithEvents btnSend As System.Windows.Forms.Button
	Friend WithEvents txt As System.Windows.Forms.TextBox
End Class
