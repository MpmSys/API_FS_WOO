Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports Newtonsoft.Json
Imports System.Linq

Public Class FrmMenu

    Public BaseDatos As String
    Dim CadenaConexion As String
    Dim Conexion As OleDb.OleDbConnection

    Dim ConsultaAcc1 As OleDb.OleDbCommand
    Dim ReaderAcc1 As OleDb.OleDbDataReader

    Dim ConsultaAcc2 As OleDb.OleDbCommand
    Dim ReaderAcc2 As OleDb.OleDbDataReader

    Dim ConsultaAcc3 As OleDb.OleDbCommand
    Dim ReaderAcc3 As OleDb.OleDbDataReader

    Dim X As Integer
    Dim WebFs As String
    Dim WebWoo As String
    Dim ClaveFs As String
    Dim ClaveWoo As String
    Dim PassWoo As String

    Private Sub FrmMenu_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim Fichero As String

        Text = My.Application.Info.Title & " - " & My.Application.Info.Version.ToString & " " &
            My.Computer.Name & " " & My.User.Name
        '
        ' Localizacion del directorio BaseDatos
        '
        Fichero = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\MpmSystems\BaseApi.cfg"
        BaseDatos = ""

        If My.Computer.FileSystem.FileExists(Fichero) Then
            Dim ObjFicheroL As New StreamReader(Fichero)
            BaseDatos = ObjFicheroL.ReadLine()
            ObjFicheroL.Close()
            ObjFicheroL.Dispose()
        End If

        If BaseDatos = "" Then
            Call MsgBox("Seleccione Base de Datos", vbOKOnly + vbCritical, "Nueva Instalación")
            Cmd.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            Cmd.Filter = "Ficheros de datos|*.mdb"
            Cmd.FileName = "Api_Woocommerce.mdb"
            Cmd.ShowDialog()

            BaseDatos = Cmd.FileName

            For X = Len(BaseDatos) To 1 Step -1
                If Mid(BaseDatos, X, 1) = "\" Then Exit For
            Next

            BaseDatos = Mid(BaseDatos, 1, X)

            Dim ObjFicheroE As New StreamWriter(Fichero)
            ObjFicheroE.WriteLine(BaseDatos)
            ObjFicheroE.Close()
            ObjFicheroE.Dispose()
        End If

        TextApi.Text = BaseDatos
        '
        ' Leemos la tabla API para obtener las rutas de WooCommerce y Facturascripts y las claves
        '
        Dim FicheroApi As String
        FicheroApi = BaseDatos & "\Api_Woocommerce.mdb"

        If My.Computer.FileSystem.FileExists(FicheroApi) Then
            Dim ObjFicheroApi As New OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & FicheroApi)
            ObjFicheroApi.Open()

            ConsultaAcc1 = New OleDb.OleDbCommand("SELECT * FROM API", ObjFicheroApi)
            ReaderAcc1 = ConsultaAcc1.ExecuteReader()

            If ReaderAcc1.HasRows Then
                ReaderAcc1.Read()
                WebFs = ReaderAcc1("URLFS").ToString()
                WebWoo = ReaderAcc1("URLWOO").ToString()
                ClaveFs = ReaderAcc1("API").ToString()
                ClaveWoo = ReaderAcc1("CK").ToString()
                PassWoo = ReaderAcc1("CS").ToString()

                Traza.Text = WebFs & vbCrLf & WebWoo & vbCrLf
            End If

            ReaderAcc1.Close()
            ObjFicheroApi.Close()
        End If

        Traza.Refresh()

    End Sub

    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnSalir.Click
        End
    End Sub

    Private Async Sub BtnDescargar_Click(sender As Object, e As EventArgs) Handles BtnDescargar.Click

        ' Configuración de la API de WooCommerce
        Dim BaseUrl As String = WebWoo.TrimEnd("/"c) & "/wp-json/wc/v3/"
        Dim ConsumerKey As String = ClaveWoo
        Dim ConsumerSecret As String = PassWoo

        ' Seguridad: TLS 1.2 (requerido por la mayoría de hosts)
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        ' Cliente HTTP con descompresión automática
        Dim handler As New HttpClientHandler() With {
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }

        Using client As New HttpClient(handler)
            client.Timeout = TimeSpan.FromSeconds(60)
            client.BaseAddress = New Uri(BaseUrl)

            ' Encabezados comunes
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)")

            Dim page As Integer = 1
            Dim totalPages As Integer = 1
            Dim pedidos As New List(Of Object)()

            Traza.AppendText("Iniciando descarga de pedidos..." & vbCrLf)
            Traza.Refresh()

            Try
                Do
                    ' ✅ Credenciales como parámetros de URL (como funciona en el navegador)
                    Dim url = $"orders?consumer_key={ConsumerKey}&consumer_secret={ConsumerSecret}&per_page=50&page={page}&orderby=id&order=asc"

                    Traza.AppendText($"Solicitando página {page}..." & vbCrLf)
                    Traza.Refresh()

                    Dim resp As HttpResponseMessage = Await client.GetAsync(url).ConfigureAwait(True)

                    Traza.AppendText($"Status Code: {CInt(resp.StatusCode)} {resp.StatusCode}" & vbCrLf)
                    Traza.Refresh()

                    If Not resp.IsSuccessStatusCode Then
                        Dim bodyErr = Await resp.Content.ReadAsStringAsync().ConfigureAwait(True)
                        Traza.AppendText($"Error: {bodyErr}" & vbCrLf)
                        Traza.Refresh()
                        Exit Do
                    End If

                    ' Paginación desde cabeceras (si están presentes)
                    If resp.Headers.Contains("X-WP-TotalPages") Then
                        Integer.TryParse(resp.Headers.GetValues("X-WP-TotalPages").FirstOrDefault(), totalPages)
                    End If

                    Dim json As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(True)
                    Dim pageOrders As List(Of Object) = JsonConvert.DeserializeObject(Of List(Of Object))(json)

                    pedidos.AddRange(pageOrders)

                    Traza.AppendText($"Descargados {pageOrders.Count} pedidos (página {page} de {totalPages})" & vbCrLf)

                    ' Si el host no envía X-WP-TotalPages, cortamos por cuenta
                    If Not resp.Headers.Contains("X-WP-TotalPages") AndAlso pageOrders.Count < 50 Then
                        Exit Do
                    End If

                    page += 1
                Loop While page <= totalPages

            Catch ex As TaskCanceledException
                Traza.AppendText($"Timeout: {ex.Message}" & vbCrLf)
                MsgBox("Se agotó el tiempo de espera al conectar con la API.", vbExclamation, "Timeout")
            Catch ex As Exception
                Traza.AppendText($"Excepción: {ex.Message}" & vbCrLf & ex.StackTrace & vbCrLf)
                MsgBox($"Error de conexión: {ex.Message}", vbCritical, "Error")
            End Try

            Traza.AppendText(vbCrLf & $"Total de pedidos descargados: {pedidos.Count}" & vbCrLf & vbCrLf)

            ' Muestra cada pedido
            For Each pedido In pedidos
                Traza.AppendText(JsonConvert.SerializeObject(pedido) & vbCrLf & vbCrLf)
            Next
        End Using
    End Sub


End Class
