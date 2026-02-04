Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports Newtonsoft.Json
Imports System.Text

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

    Private Sub BtnDescargar_Click(sender As Object, e As EventArgs) Handles BtnDescargar.Click

        Dim BaseUrl As String = WebWoo & "/wp-json/wc/v3/"
        Dim ConsumerKey As String = ClaveWoo
        Dim ConsumerSecret As String = PassWoo

        Debug.Print("BaseUrl: " & BaseUrl)
        Debug.Print("ConsumerKey: " & ConsumerKey)
        Debug.Print("ConsumerSecret: " & ConsumerSecret)

        ' Crear HttpClientHandler para manejar SSL/TLS
        Dim handler As New HttpClientHandler()
        handler.ServerCertificateCustomValidationCallback = Function(sender2, cert, chain, sslPolicyErrors) True

        Dim httpClient As New HttpClient(handler)
        httpClient.Timeout = TimeSpan.FromSeconds(60)

        ' Configurar headers
        httpClient.DefaultRequestHeaders.Accept.Clear()
        httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")

        ' NO usar Basic Auth - WooCommerce prefiere credenciales en URL
        ' Agregar headers adicionales
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache")

        Dim page As Integer = 1
        Dim pedidos As New List(Of Object)()
        Dim morePages As Boolean = True

        ' Bucle para obtener todas las páginas de pedidos
        Traza.Text = Traza.Text & "Iniciando descarga de pedidos..." & vbCrLf
        Traza.Refresh()

        While morePages

            ' URL CON credenciales en query string (método recomendado para WooCommerce)
            Dim url As String = $"{BaseUrl}orders?consumer_key={ConsumerKey}&consumer_secret={ConsumerSecret}&per_page=50&page={page}&orderby=id&order=asc"

            Traza.Text = Traza.Text & $"Solicitando página {page}..." & vbCrLf
            Traza.Refresh()

            Try
                ' Realizar la solicitud GET de forma asíncrona
                Dim response As HttpResponseMessage = httpClient.GetAsync(url).Result

                ' Mostrar información de respuesta para debugging
                Traza.Text = Traza.Text & $"Status Code: {response.StatusCode}" & vbCrLf

                If response.IsSuccessStatusCode Then
                    Dim jsonResponse As String = response.Content.ReadAsStringAsync().Result
                    Dim pageOrders As List(Of Object) = JsonConvert.DeserializeObject(Of List(Of Object))(jsonResponse)

                    ' Agregar los pedidos obtenidos a la lista principal
                    pedidos.AddRange(pageOrders)

                    Traza.Text = Traza.Text & $"Descargados {pageOrders.Count} pedidos de la página {page}" & vbCrLf
                    Traza.Refresh()

                    ' Verificar si hay más páginas
                    If pageOrders.Count < 50 Then
                        morePages = False ' No hay más páginas si se obtienen menos de 50 pedidos
                    Else
                        page += 1 ' Incrementar el número de página para la siguiente solicitud
                    End If
                Else
                    ' Mostrar información detallada del error   
                    Dim errorContent As String = response.Content.ReadAsStringAsync().Result
                    Traza.Text = Traza.Text & $"Error {response.StatusCode}: {response.ReasonPhrase}" & vbCrLf
                    Traza.Text = Traza.Text & $"Detalle: {errorContent}" & vbCrLf
                    Traza.Refresh()
                    morePages = False ' Detener el bucle en caso de error en la solicitud

                    MsgBox($"Error al descargar pedidos: {response.StatusCode} - {response.ReasonPhrase}" & vbCrLf & vbCrLf & "Verifique:" & vbCrLf & "- Consumer Key y Secret correctos" & vbCrLf & "- Permisos de lectura en WooCommerce" & vbCrLf & "- URL correcta", vbCritical, "Error de API")
                End If

            Catch ex As Exception
                Traza.Text = Traza.Text & $"Excepción: {ex.Message}" & vbCrLf
                Traza.Text = Traza.Text & $"Stack Trace: {ex.StackTrace}" & vbCrLf
                Traza.Refresh()
                morePages = False

                MsgBox($"Error de conexión: {ex.Message}", vbCritical, "Error")
            End Try

        End While

        ' Procesar los pedidos obtenidos
        Traza.Text = Traza.Text & vbCrLf & $"Total de pedidos descargados: {pedidos.Count}" & vbCrLf & vbCrLf
        Traza.Refresh()

        For Each pedido In pedidos
            ' Extraemos los campos
            ' Id del pedido
            ' Status del pedido
            ' date_created
            ' payment_method
            ' payment_method_title
            ' date_paid
            ' total
            ' total_tax

            Traza.Text = Traza.Text & Newtonsoft.Json.JsonConvert.SerializeObject(pedido) & vbCrLf & vbCrLf
            Traza.Refresh()

        Next

        httpClient.Dispose()

    End Sub

End Class
