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
        Me.SuspendLayout()
        '
        'Traza
        '
        Me.Traza.Font = New System.Drawing.Font("Courier New", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Traza.Location = New System.Drawing.Point(145, 48)
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
        Me.BtnSalir.TabIndex = 42
        Me.BtnSalir.Text = "Salir"
        Me.BtnSalir.UseVisualStyleBackColor = True
        '
        'TextApi
        '
        Me.TextApi.AutoSize = True
        Me.TextApi.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextApi.Location = New System.Drawing.Point(141, 12)
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
        Me.BtnDescargar.Image = CType(resources.GetObject("BtnDescargar.Image"), System.Drawing.Image)
        Me.BtnDescargar.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnDescargar.Location = New System.Drawing.Point(12, 96)
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
        Me.BtnStock.Location = New System.Drawing.Point(12, 404)
        Me.BtnStock.Name = "BtnStock"
        Me.BtnStock.Size = New System.Drawing.Size(123, 68)
        Me.BtnStock.TabIndex = 50
        Me.BtnStock.Text = "Descargar Stock"
        Me.BtnStock.UseVisualStyleBackColor = True
        '
        'BtnActualizar
        '
        Me.BtnActualizar.Image = CType(resources.GetObject("BtnActualizar.Image"), System.Drawing.Image)
        Me.BtnActualizar.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.BtnActualizar.Location = New System.Drawing.Point(12, 180)
        Me.BtnActualizar.Name = "BtnActualizar"
        Me.BtnActualizar.Size = New System.Drawing.Size(123, 68)
        Me.BtnActualizar.TabIndex = 51
        Me.BtnActualizar.Text = "Actualizar los Pedidos"
        Me.BtnActualizar.UseVisualStyleBackColor = True
        '
        'FrmMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1168, 589)
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
End Class
