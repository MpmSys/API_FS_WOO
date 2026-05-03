<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMenu
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmMenu))
        Me.Traza = New System.Windows.Forms.TextBox()
        Me.BtnSalir = New System.Windows.Forms.Button()
        Me.TextApi = New System.Windows.Forms.Label()
        Me.Cmd = New System.Windows.Forms.OpenFileDialog()
        Me.BtnDescargar = New System.Windows.Forms.Button()
        Me.BtnStock = New System.Windows.Forms.Button()
        Me.BtnActualizar = New System.Windows.Forms.Button()
        Me.BtnFacturar = New System.Windows.Forms.Button()
        Me.BtnBoceto = New System.Windows.Forms.Button()
        Me.BtnClientes = New System.Windows.Forms.Button()
        Me.BtnPedido = New System.Windows.Forms.Button()
        Me.BtnPrint = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Traza
        '
        Me.Traza.Font = New System.Drawing.Font("Courier New", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Traza.Location = New System.Drawing.Point(302, 42)
        Me.Traza.Multiline = True
        Me.Traza.Name = "Traza"
        Me.Traza.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.Traza.Size = New System.Drawing.Size(1011, 535)
        Me.Traza.TabIndex = 43
        '
        'BtnSalir
        '
        Me.BtnSalir.Image = CType(resources.GetObject("BtnSalir.Image"), System.Drawing.Image)
        Me.BtnSalir.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnSalir.Location = New System.Drawing.Point(12, 12)
        Me.BtnSalir.Name = "BtnSalir"
        Me.BtnSalir.Size = New System.Drawing.Size(123, 68)
        Me.BtnSalir.TabIndex = 1
        Me.BtnSalir.Text = "Salir"
        Me.BtnSalir.UseVisualStyleBackColor = True
        '
        'TextApi
        '
        Me.TextApi.AutoSize = True
        Me.TextApi.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextApi.Location = New System.Drawing.Point(298, 12)
        Me.TextApi.Name = "TextApi"
        Me.TextApi.Size = New System.Drawing.Size(62, 20)
        Me.TextApi.TabIndex = 45
        Me.TextApi.Text = "TextApi"
        '
        'Cmd
        '
        Me.Cmd.FileName = "Cmd"
        '
        'BtnDescargar
        '
        Me.BtnDescargar.Enabled = False
        Me.BtnDescargar.Image = CType(resources.GetObject("BtnDescargar.Image"), System.Drawing.Image)
        Me.BtnDescargar.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnDescargar.Location = New System.Drawing.Point(152, 12)
        Me.BtnDescargar.Name = "BtnDescargar"
        Me.BtnDescargar.Size = New System.Drawing.Size(123, 68)
        Me.BtnDescargar.TabIndex = 49
        Me.BtnDescargar.Text = "Descargar Todos los Pedidos"
        Me.BtnDescargar.UseVisualStyleBackColor = True
        '
        'BtnStock
        '
        Me.BtnStock.Image = CType(resources.GetObject("BtnStock.Image"), System.Drawing.Image)
        Me.BtnStock.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnStock.Location = New System.Drawing.Point(12, 160)
        Me.BtnStock.Name = "BtnStock"
        Me.BtnStock.Size = New System.Drawing.Size(123, 68)
        Me.BtnStock.TabIndex = 50
        Me.BtnStock.Text = "Revisar Stock Cajas"
        Me.BtnStock.UseVisualStyleBackColor = True
        '
        'BtnActualizar
        '
        Me.BtnActualizar.Image = CType(resources.GetObject("BtnActualizar.Image"), System.Drawing.Image)
        Me.BtnActualizar.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnActualizar.Location = New System.Drawing.Point(12, 86)
        Me.BtnActualizar.Name = "BtnActualizar"
        Me.BtnActualizar.Size = New System.Drawing.Size(123, 68)
        Me.BtnActualizar.TabIndex = 51
        Me.BtnActualizar.Text = "Actualizar los Pedidos"
        Me.BtnActualizar.UseVisualStyleBackColor = True
        '
        'BtnFacturar
        '
        Me.BtnFacturar.Enabled = False
        Me.BtnFacturar.Image = CType(resources.GetObject("BtnFacturar.Image"), System.Drawing.Image)
        Me.BtnFacturar.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnFacturar.Location = New System.Drawing.Point(152, 86)
        Me.BtnFacturar.Name = "BtnFacturar"
        Me.BtnFacturar.Size = New System.Drawing.Size(123, 68)
        Me.BtnFacturar.TabIndex = 52
        Me.BtnFacturar.Text = "Facturar"
        Me.BtnFacturar.UseVisualStyleBackColor = True
        '
        'BtnBoceto
        '
        Me.BtnBoceto.Enabled = False
        Me.BtnBoceto.Image = CType(resources.GetObject("BtnBoceto.Image"), System.Drawing.Image)
        Me.BtnBoceto.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnBoceto.Location = New System.Drawing.Point(152, 160)
        Me.BtnBoceto.Name = "BtnBoceto"
        Me.BtnBoceto.Size = New System.Drawing.Size(123, 68)
        Me.BtnBoceto.TabIndex = 53
        Me.BtnBoceto.Text = "Paso a Boceto"
        Me.BtnBoceto.UseVisualStyleBackColor = True
        '
        'BtnClientes
        '
        Me.BtnClientes.Enabled = False
        Me.BtnClientes.Image = CType(resources.GetObject("BtnClientes.Image"), System.Drawing.Image)
        Me.BtnClientes.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnClientes.Location = New System.Drawing.Point(152, 234)
        Me.BtnClientes.Name = "BtnClientes"
        Me.BtnClientes.Size = New System.Drawing.Size(123, 68)
        Me.BtnClientes.TabIndex = 54
        Me.BtnClientes.Text = "Control Clientes"
        Me.BtnClientes.UseVisualStyleBackColor = True
        '
        'BtnPedido
        '
        Me.BtnPedido.Enabled = False
        Me.BtnPedido.Image = CType(resources.GetObject("BtnPedido.Image"), System.Drawing.Image)
        Me.BtnPedido.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnPedido.Location = New System.Drawing.Point(152, 308)
        Me.BtnPedido.Name = "BtnPedido"
        Me.BtnPedido.Size = New System.Drawing.Size(123, 68)
        Me.BtnPedido.TabIndex = 55
        Me.BtnPedido.Text = "Leer Pedido"
        Me.BtnPedido.UseVisualStyleBackColor = True
        '
        'BtnPrint
        '
        Me.BtnPrint.Image = CType(resources.GetObject("BtnPrint.Image"), System.Drawing.Image)
        Me.BtnPrint.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnPrint.Location = New System.Drawing.Point(12, 234)
        Me.BtnPrint.Name = "BtnPrint"
        Me.BtnPrint.Size = New System.Drawing.Size(123, 142)
        Me.BtnPrint.TabIndex = 56
        Me.BtnPrint.Text = "Imprimir Textos"
        Me.BtnPrint.UseVisualStyleBackColor = True
        '
        'FrmMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1329, 606)
        Me.Controls.Add(Me.BtnPrint)
        Me.Controls.Add(Me.BtnPedido)
        Me.Controls.Add(Me.BtnClientes)
        Me.Controls.Add(Me.BtnBoceto)
        Me.Controls.Add(Me.BtnFacturar)
        Me.Controls.Add(Me.BtnActualizar)
        Me.Controls.Add(Me.BtnStock)
        Me.Controls.Add(Me.BtnDescargar)
        Me.Controls.Add(Me.TextApi)
        Me.Controls.Add(Me.Traza)
        Me.Controls.Add(Me.BtnSalir)
        Me.Name = "FrmMenu"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Traza As TextBox
    Friend WithEvents BtnSalir As Button
    Friend WithEvents TextApi As Label
    Friend WithEvents Cmd As OpenFileDialog
    Friend WithEvents BtnDescargar As Button
    Friend WithEvents BtnStock As Button
    Friend WithEvents BtnActualizar As Button
    Friend WithEvents BtnFacturar As Button
    Friend WithEvents BtnBoceto As Button
    Friend WithEvents BtnClientes As Button
    Friend WithEvents BtnPedido As Button
    Friend WithEvents BtnPrint As Button
End Class
