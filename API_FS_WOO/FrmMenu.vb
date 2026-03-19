Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Policy
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

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
    Dim WebId As String
    Dim WebFs As String
    Dim WebWoo As String
    Dim ClaveFs As String
    Dim ClaveWoo As String
    Dim PassWoo As String

    Dim TotalReg As Long
    Dim NumReg As Long
    Dim ApiVal As String
    Dim Offset As Long

    Dim OldTraza As String

    Public Class CRUDMethod
        Public Shared Create As String = "POST"
        Public Shared Read As String = "GET"
        Public Shared Update As String = "PUT"
        Public Shared Delete As String = "DELETE"
    End Class

    Public Function LeerApi(RequestURL As String, Optional ApiOrigen As String = "") As String

        Dim WebRequest As HttpWebRequest
        Dim Reader As StreamReader
        Dim Response As HttpWebResponse

        Dim ApiLoc As String
        Dim StrData As String

        ' Inicializar contadores para evitar valores previos
        TotalReg = 0
        NumReg = 0

        TextApi.Text = BaseDatos
        TextApi.Refresh()
        '
        ' Lectura API
        '
        Debug.Print(RequestURL)

        ApiLoc = "Token:" & ApiVal
        If ApiOrigen <> "" Then
            ApiLoc = "Token:" & ApiOrigen
        End If

        WebRequest = DirectCast(System.Net.WebRequest.Create(RequestURL), HttpWebRequest)
        WebRequest.Method = CRUDMethod.Read
        WebRequest.Headers.Add(ApiLoc)

        'Traza.Text += "Leyendo API " & RequestURL & vbCrLf
        'Traza.Text += "Token utilizado: " & ApiLoc & vbCrLf
        'Traza.Refresh()

        Try
            '
            ' Realizamos la petición y devolvemos los siguientes datos
            ' StrData = Respuesta de la API
            ' TotalReg = Número total de registros (cabecera X-Total-Count)
            ' NumReg = Número de registros devueltos en la consulta
            '
            Response = DirectCast(WebRequest.GetResponse(), HttpWebResponse)
            Reader = New StreamReader(Response.GetResponseStream())
            StrData = Reader.ReadToEnd()
            Reader.Close()

            ' Intentar leer la cabecera X-Total-Count
            Try
                Dim totalHeader As String = Response.Headers("X-Total-Count")
                If Not String.IsNullOrEmpty(totalHeader) Then
                    Long.TryParse(totalHeader, TotalReg)
                End If
            Catch exHdr As Exception
                ' Si no existe o no se puede parsear, mantener TotalReg = 0
            End Try

            ' Intentar contar registros si la respuesta es un array JSON
            If Not String.IsNullOrWhiteSpace(StrData) Then
                Try
                    Dim lecturaJson As JArray = JArray.Parse(StrData.ToString)
                    NumReg = lecturaJson.Count
                Catch exJson As Exception
                    ' No es un array JSON o parseo fallido: dejar NumReg = 0
                End Try
            End If

        Catch ex As Exception
            Traza.Text += "LeerApi FacturaScript " & vbCrLf & RequestURL & vbCrLf & ex.ToString & vbCrLf
            StrData = ""
        End Try

        Debug.Print(StrData)
        LeerApi = StrData

    End Function

    Public Function ActualizarApi(RequestURL As String, StrData As String) As String

        Dim WebRequest As HttpWebRequest
        Dim Reader As StreamReader
        Dim Response As HttpWebResponse
        Dim StrDataRes As String = ""

        Dim ApiLoc As String

        ' Inicializar contadores para evitar valores previos
        TotalReg = 0
        NumReg = 0

        TextApi.Text = BaseDatos
        TextApi.Refresh()
        '
        ' Preparar la petición para enviar datos como form URL encoded
        '
        Debug.Print(RequestURL)

        ApiLoc = "Token:" & ApiVal

        WebRequest = DirectCast(System.Net.WebRequest.Create(RequestURL), HttpWebRequest)
        WebRequest.Method = CRUDMethod.Update
        WebRequest.Headers.Add(ApiLoc)

        ' Convertir StrData JSON a application/x-www-form-urlencoded si es necesario
        Dim postData As String = ""
        If Not String.IsNullOrWhiteSpace(StrData) Then
            Try
                ' Intentar parsear como JSON
                Dim jo As JObject = JObject.Parse(StrData)
                Dim parts As New System.Collections.Generic.List(Of String)
                For Each prop As JProperty In jo.Properties()
                    Dim name As String = prop.Name
                    Dim value As String = If(prop.Value Is Nothing, "", prop.Value.ToString())
                    parts.Add(Uri.EscapeDataString(name) & "=" & Uri.EscapeDataString(value))
                Next
                postData = String.Join("&", parts)
            Catch ex As Exception
                ' No es JSON: asumir que ya es form-url-encoded
                postData = StrData
            End Try
        End If

        WebRequest.ContentType = "application/x-www-form-urlencoded"
        Dim dataBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(postData)
        WebRequest.ContentLength = dataBytes.Length

        Try
            Using requestStream As System.IO.Stream = WebRequest.GetRequestStream()
                requestStream.Write(dataBytes, 0, dataBytes.Length)
                requestStream.Flush()
            End Using

            '
            ' procesamos la perciobn WebRequest y recogemos la respuesta
            '
            Response = DirectCast(WebRequest.GetResponse(), HttpWebResponse)
            Reader = New StreamReader(Response.GetResponseStream())
            StrDataRes = Reader.ReadToEnd()
            Reader.Close()

        Catch ex As Exception
            Traza.Text += "ActualizarApi FacturaScript " & vbCrLf & RequestURL & vbCrLf & ex.ToString & vbCrLf
            StrDataRes = ""
        End Try

        Debug.Print(StrDataRes)
        ActualizarApi = StrDataRes

    End Function

    Private Function NormalizeJsonString(input As String) As String
        If String.IsNullOrWhiteSpace(input) Then
            Return input
        End If

        Dim s As String = input.Trim()

        ' Reemplazar True/False por true/false (coincidencia de palabra, case-insensitive)
        s = System.Text.RegularExpressions.Regex.Replace(s, "\bTrue\b", "true", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        s = System.Text.RegularExpressions.Regex.Replace(s, "\bFalse\b", "false", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' Reemplazar comillas simples por dobles (solo cuando parezcan delimitar cadenas)
        ' Evitar tocar apostrofes dentro de cadenas ya entre dobles.
        s = s.Replace("'", """")

        ' Eliminar comas finales antes de } o ]
        s = System.Text.RegularExpressions.Regex.Replace(s, ",\s*([\}\]])", "$1")

        Return s
    End Function

    Public Function CrearApi(RequestURL As String, StrData As String, Optional ApiOrigen As String = "") As String
        Dim WebRequest As HttpWebRequest
        Dim Reader As StreamReader
        Dim Response As HttpWebResponse
        Dim StrDataRes As String = ""
        Dim ApiLoc As String

        TotalReg = 0
        NumReg = 0
        TextApi.Text = BaseDatos
        TextApi.Refresh()

        Debug.Print(RequestURL)

        ApiLoc = "Token:" & ApiVal
        If ApiOrigen <> "" Then
            ApiLoc = "Token:" & ApiOrigen
        End If

        WebRequest = DirectCast(System.Net.WebRequest.Create(RequestURL), HttpWebRequest)
        WebRequest.Method = CRUDMethod.Create
        WebRequest.Headers.Add(ApiLoc)

        ' Convertir JSON a form-url-encoded
        Dim postData As String = ""
        If Not String.IsNullOrWhiteSpace(StrData) Then
            Try
                Dim jo As JObject = JObject.Parse(StrData)
                Dim parts As New System.Collections.Generic.List(Of String)
                For Each prop As JProperty In jo.Properties()
                    Dim name As String = prop.Name
                    Dim value As String = If(prop.Value Is Nothing, "", prop.Value.ToString())
                    'parts.Add(Uri.EscapeDataString(name) & "=" & Uri.EscapeDataString(value))

                    parts.Add(name & "=" & value)


                Next
                postData = String.Join("&", parts)
            Catch ex As Exception
                postData = StrData
            End Try
        End If

        ' ✅ Logging para debug
        Debug.Print("POST Data: " & postData)
        Debug.Print("Token: " & ApiLoc)

        WebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8"
        Dim dataBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(postData)
        WebRequest.ContentLength = dataBytes.Length

        Try
            Using requestStream As System.IO.Stream = WebRequest.GetRequestStream()
                requestStream.Write(dataBytes, 0, dataBytes.Length)
                requestStream.Flush()
            End Using

            Response = DirectCast(WebRequest.GetResponse(), HttpWebResponse)
            Reader = New StreamReader(Response.GetResponseStream())
            StrDataRes = Reader.ReadToEnd()
            Reader.Close()

        Catch ex As WebException
            ' ✅ Capturar detalles completos del error 400
            If ex.Response IsNot Nothing Then
                Using errResponse As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                    Using errReader As New StreamReader(errResponse.GetResponseStream())
                        Dim errorBody As String = errReader.ReadToEnd()

                        Traza.Text += "━━━ ERROR CrearApi ━━━" & vbCrLf
                        Traza.Text += "URL: " & RequestURL & vbCrLf
                        Traza.Text += "Status: " & errResponse.StatusCode.ToString() & vbCrLf
                        Traza.Text += "Datos enviados: " & postData & vbCrLf
                        Traza.Text += "Respuesta del servidor: " & errorBody & vbCrLf
                        Traza.Text += "━━━━━━━━━━━━━━━━━━━━━━" & vbCrLf

                        Debug.Print("Error Response: " & errorBody)
                    End Using
                End Using
            Else
                Traza.Text += "CrearApi Error (sin respuesta): " & ex.Message & vbCrLf
            End If
            StrDataRes = ""

        Catch ex As Exception
            Traza.Text += "CrearApi Excepción: " & ex.ToString & vbCrLf
            StrDataRes = ""
        End Try

        Debug.Print(StrDataRes)
        Return StrDataRes
    End Function

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
                WebId = ReaderAcc1("IDWEB").ToString()

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

    Private Sub ProcesarPedido(pedido As Object, conn As OleDb.OleDbConnection)
        ' Mostramos los valores de los campos principales del pedido
        ' Id - status - total - fecha de creación - metodo de pago - titulo de metodo de pago y fecha de pago

        Dim idpedido As String = pedido("id").ToString()
        Dim status As String = pedido("status").ToString()
        Dim total As String = pedido("total").ToString()
        Dim fechaCreacion As String = pedido("date_created").ToString()
        Dim metodoPago As String = pedido("payment_method").ToString()
        Dim tituloMetodoPago As String = pedido("payment_method_title").ToString()
        Dim fechaPago As String = pedido("date_paid").ToString()
        Dim fechacierre As String = pedido("date_completed").ToString()

        'Traza.AppendText($"ID: {idpedido}, Status: {status}, Total: {total}, Fecha Creación: {fechaCreacion}, " &
        '                 $"Método Pago: {metodoPago} ({tituloMetodoPago}), Fecha Pago: {fechaPago}" & vbCrLf)
        Traza.AppendText($"ID: {idpedido}, Status: {status}" & vbCrLf)
        Traza.Refresh()

        ' si el pedido esta en la tabla Pedidos , lo actualizamos, si no, lo insertamos.
        ' Para esto, usamos el los valores IdWeb y el campo ID del pedido como referencia llave doble para identificar el pedido.
        Dim cmd As New OleDb.OleDbCommand("SELECT COUNT(*) FROM Pedidos WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
        cmd.Parameters.AddWithValue("@IdWeb", WebId)
        cmd.Parameters.AddWithValue("@IdPedido", pedido("id").ToString())
        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())

        ' si la fecha de pago es vacia, la ponemos a 01/01/1900 para evitar errores de conversion
        If String.IsNullOrEmpty(fechaPago) Then
            fechaPago = "01/01/1900"
        End If

        ' si la fecha de cierre es vacia, la ponemos a 01/01/1900 para evitar errores de conversion
        If String.IsNullOrEmpty(fechacierre) Then
            fechacierre = "01/01/1900"
        End If

        ' el valor de total lo convertimos a double. Hay que cambiar el punto por la coma si es necesario
        Dim totalDouble As Double
        If Not Double.TryParse(total.Replace(".", ","), totalDouble) Then
            totalDouble = 0
        End If

        If count > 0 Then
            ' Actualizar pedido existente (NO modificar Estado)
            Dim updateCmd As New OleDb.OleDbCommand("UPDATE Pedidos SET Status = @Status, Total = @Total, FechaCreacion = @FechaCreacion, " &
                                                     "MetodoPago = @MetodoPago, TituloMetodoPago = @TituloMetodoPago, FechaPago = @FechaPago, FechaCierre = @FechaCierre " &
                                                     "WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
            updateCmd.Parameters.AddWithValue("@Status", status)
            updateCmd.Parameters.AddWithValue("@Total", totalDouble)
            updateCmd.Parameters.AddWithValue("@FechaCreacion", fechaCreacion)
            updateCmd.Parameters.AddWithValue("@MetodoPago", metodoPago)
            updateCmd.Parameters.AddWithValue("@TituloMetodoPago", tituloMetodoPago)
            updateCmd.Parameters.AddWithValue("@FechaPago", fechaPago)
            updateCmd.Parameters.AddWithValue("@FechaCierre", fechacierre)
            updateCmd.Parameters.AddWithValue("@IdWeb", WebId)
            updateCmd.Parameters.AddWithValue("@IdPedido", pedido("id").ToString())
            updateCmd.ExecuteNonQuery()
        Else
            ' Insertar nuevo pedido con Estado = NULL explícitamente
            Dim insertCmd As New OleDb.OleDbCommand("INSERT INTO Pedidos (IdWeb, IdPedido, Status, Total, FechaCreacion, MetodoPago, TituloMetodoPago, FechaPago, FechaCierre, Estado) " &
                                                     "VALUES (@IdWeb, @IdPedido, @Status, @Total, @FechaCreacion, @MetodoPago, @TituloMetodoPago, @FechaPago, @FechaCierre, @Estado)", conn)
            insertCmd.Parameters.AddWithValue("@IdWeb", WebId)
            insertCmd.Parameters.AddWithValue("@IdPedido", pedido("id").ToString())
            insertCmd.Parameters.AddWithValue("@Status", status)
            insertCmd.Parameters.AddWithValue("@Total", totalDouble)
            insertCmd.Parameters.AddWithValue("@FechaCreacion", fechaCreacion)
            insertCmd.Parameters.AddWithValue("@MetodoPago", metodoPago)
            insertCmd.Parameters.AddWithValue("@TituloMetodoPago", tituloMetodoPago)
            insertCmd.Parameters.AddWithValue("@FechaPago", fechaPago)
            insertCmd.Parameters.AddWithValue("@FechaCierre", fechacierre)
            insertCmd.Parameters.AddWithValue("@Estado", DBNull.Value)
            insertCmd.ExecuteNonQuery()
        End If

        ' si status es "cancelled" y estado es NULL marcamos el pedido como "CANCELADO"
        If status = "cancelled" Or status = "trash" Then
            Dim cmdEstado As New OleDb.OleDbCommand("SELECT Estado FROM Pedidos WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
            cmdEstado.Parameters.AddWithValue("@IdWeb", WebId)
            cmdEstado.Parameters.AddWithValue("@IdPedido", pedido("id").ToString())
            Dim estadoActual = cmdEstado.ExecuteScalar()

            ' Verificar si Estado es NULL o vacío
            If IsDBNull(estadoActual) OrElse String.IsNullOrEmpty(CStr(estadoActual)) Then
                Dim updateCmd As New OleDb.OleDbCommand("UPDATE Pedidos SET Estado = 'CANCELADO', Fecha = Now() " &
                                                        "WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
                updateCmd.Parameters.AddWithValue("@IdWeb", WebId)
                updateCmd.Parameters.AddWithValue("@IdPedido", pedido("id").ToString())
                Debug.Print(updateCmd.CommandText)
                updateCmd.ExecuteNonQuery()
                Traza.Text = OldTraza
                Traza.AppendText($"Pedido {pedido("id").ToString()} marcado como CANCELADO." & vbCrLf)
                OldTraza = Traza.Text
                Traza.Refresh()
            End If
        End If

    End Sub

    Private Async Sub BtnDescargar_Click(sender As Object, e As EventArgs) Handles BtnDescargar.Click

        ' Abrimos la conexión a la base de datos para cargar o mantener la tabla Pedidos

        Dim connString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & BaseDatos & "\Api_Woocommerce.mdb"
        Dim conn As New OleDb.OleDbConnection(connString)
        conn.Open()

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

            Traza.Text = ""
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

                    'If page > 3 Then Exit Do ' Límite de páginas para pruebas

                Loop While page <= totalPages

            Catch ex As TaskCanceledException
                Traza.AppendText($"Timeout: {ex.Message}" & vbCrLf)
                MsgBox("Se agotó el tiempo de espera al conectar con la API.", vbExclamation, "Timeout")
            Catch ex As Exception
                Traza.AppendText($"Excepción: {ex.Message}" & vbCrLf & ex.StackTrace & vbCrLf)
                MsgBox($"Error de conexión: {ex.Message}", vbCritical, "Error")
            End Try

            Traza.Text = "" ' Limpiar traza antes de mostrar resultados
            Traza.AppendText(vbCrLf & $"Total de pedidos descargados: {pedidos.Count}" & vbCrLf & vbCrLf)

            ' Muestra cada pedido
            For Each pedido In pedidos
                ProcesarPedido(pedido, conn)
            Next
        End Using

        ' Cerramos la conexión a la base de datos
        conn.Close()

    End Sub

    Private Sub BtnStock_Click(sender As Object, e As EventArgs) Handles BtnStock.Click
        '
        Dim NumRegVariante As Long

        Traza.Text = "Descargando stock de FacturaScripts..." & vbCrLf
        Traza.Refresh()

        ApiVal = ClaveFs

        ' Abrimos la Base de datos para cargar o mantener la tabla Stock
        Dim connString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & BaseDatos & "\Api_Woocommerce.mdb"
        Dim conn As New OleDb.OleDbConnection(connString)
        conn.Open()

        ' Borramos el stock actual con el Idweb
        Dim deleteCmd As New OleDb.OleDbCommand("DELETE FROM Stock WHERE IdWeb = @IdWeb", conn)
        deleteCmd.Parameters.AddWithValue("@IdWeb", WebId)
        deleteCmd.ExecuteNonQuery()

        ' Paginacion
        Offset = 0

        Do
            ' BaseURL de la API de FacturaScripts
            Dim BaseUrl As String = WebFs.TrimEnd("/"c) & "/api/3/variantes?offset=" & Offset

            ' Obtenemos el stock de las variantes desde la API de FacturaScripts
            Dim response As String = LeerApi(BaseUrl)
            Traza.Text += "Lectura " & NumReg & " desde " & Offset & " de " & TotalReg & " disponibles" & vbCrLf
            Traza.Refresh()

            Offset += NumReg

            ' Para cada variante obtenida la grabamos en la tabla Stock de la base de datos
            If NumReg > 0 Then
                Dim lecturaJson As JArray = JArray.Parse(response)
                For Each variante As JObject In lecturaJson
                    Dim idvariante As String = variante("idvariante").ToString()
                    Dim idproducto As String = variante("idproducto").ToString()
                    Dim referencia As String = variante("referencia").ToString()
                    Dim stock As String = variante("stockfis").ToString()
                    Dim codbarras As String = variante("codbarras").ToString()
                    Dim tipocodbarras As String = variante("tipocodbarras").ToString()

                    ' recogemos el la descripcion de prodotto de la tabla productos con la API de FacturaScripts para guardarla en
                    ' la tabla Stock y tenerla disponible para futuras consultas sin necesidad de volver a llamar a la API de
                    ' FacturaScripts.
                    Dim BaseUrlProducto As String = WebFs.TrimEnd("/"c) & "/api/3/productos/" & idproducto
                    NumRegVariante = NumReg
                    Dim responseProducto As String = LeerApi(BaseUrlProducto)
                    NumReg = NumRegVariante
                    Dim lecturaJsonProducto As JObject = JObject.Parse(responseProducto)
                    Dim descripcion As String = lecturaJsonProducto("descripcion").ToString()

                    ' recogemos los valores stockmin y stockmax de la tabla stocks de la API de FacturaScripts para guardarlos
                    ' en la tabla Stock y tenerlos disponibles para futuras consultas sin necesidad de volver a llamar a la API de
                    ' FacturaScripts.
                    Dim BaseUrlStock As String = WebFs.TrimEnd("/"c) & "/api/3/stocks?referencia=" & referencia
                    NumRegVariante = NumReg
                    Dim responseStock As String = LeerApi(BaseUrlStock)
                    NumReg = NumRegVariante
                    Dim lecturaJsonStock As JArray = JArray.Parse(responseStock)
                    Dim stockmin As String = ""
                    Dim stockmax As String = ""
                    If lecturaJsonStock.Count > 0 Then
                        stockmin = lecturaJsonStock(0)("stockmin").ToString()
                        stockmax = lecturaJsonStock(0)("stockmax").ToString()
                    End If

                    ' Insertamos el stock de la variante en la tabla Stock de la base de datos

                    Dim insertCmd As New OleDb.OleDbCommand(
                        "INSERT INTO Stock (IdWeb, Variante, Stock, Producto, Referencia, Descripcion, CodBarras, TipoCodBarras, StockMin, StockMax)" &
                        "VALUES (@IdWeb, @Idvariante, @Stock, @Producto, @Referencia, @Descripcion, @codbarras, @tipocodbarras, @stockmin, @stockmax)", conn)
                    insertCmd.Parameters.AddWithValue("@IdWeb", WebId)
                    insertCmd.Parameters.AddWithValue("@Idvariante", idvariante)
                    insertCmd.Parameters.AddWithValue("@Stock", stock)
                    insertCmd.Parameters.AddWithValue("@Producto", idproducto)
                    insertCmd.Parameters.AddWithValue("@Referencia", referencia)
                    insertCmd.Parameters.AddWithValue("@Descripcion", descripcion)
                    insertCmd.Parameters.AddWithValue("@codbarras", codbarras)
                    insertCmd.Parameters.AddWithValue("@tipcodbarras", tipocodbarras)
                    insertCmd.Parameters.AddWithValue("@stockmin", stockmin)
                    insertCmd.Parameters.AddWithValue("@stockmax", stockmax)
                    insertCmd.ExecuteNonQuery()
                Next
            End If

        Loop Until NumReg = 0

        ' Cerramos la conexión a la base de datos
        conn.Close()

        Traza.Text += "Stock descargado correctamente" & vbCrLf
        Traza.Refresh()

    End Sub

    Private Async Sub BtnActualizar_Click(sender As Object, e As EventArgs) Handles BtnActualizar.Click

        Dim LastPedido As String = ""
        Dim NumPedidos As Integer = 0
        Dim MaxPedido As String = ""

        ' Abrimos la Base de datos para mantener la tabla Stock
        Dim connString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & BaseDatos & "\Api_Woocommerce.mdb"
        Dim conn As New OleDb.OleDbConnection(connString)
        conn.Open()

        ' Configuración de la API de WooCommerce
        Dim BaseUrl As String = WebWoo.TrimEnd("/"c) & "/wp-json/wc/v3/"
        Dim ConsumerKey As String = ClaveWoo
        Dim ConsumerSecret As String = PassWoo

        ' Leemos en Woocommerce cual es el último pedido para tener un punto de fin en la actualización de pedidos.

        Dim Url As String = "orders?consumer_key=" & ConsumerKey & "&consumer_secret=" & ConsumerSecret & "&per_page=1&page=1&orderby=id&order=desc"

        Dim pedidos As New List(Of Object)()

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

            Traza.AppendText("Buscando ultimo pedido..." & vbCrLf)
            Traza.Refresh()

            Try
                ' ✅ Credenciales como parámetros de URL (como funciona en el navegador)
                Dim resp As HttpResponseMessage = Await client.GetAsync(Url).ConfigureAwait(True)

                Traza.AppendText($"Status Code: {CInt(resp.StatusCode)} {resp.StatusCode}" & vbCrLf)
                Traza.Refresh()

                If Not resp.IsSuccessStatusCode Then
                    Dim bodyErr = Await resp.Content.ReadAsStringAsync().ConfigureAwait(True)
                    Traza.AppendText($"Error: {bodyErr}" & vbCrLf)
                    Traza.Refresh()
                    Exit Try
                End If

                Dim json As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(True)
                Dim pageOrders As List(Of Object) = JsonConvert.DeserializeObject(Of List(Of Object))(json)

                pedidos.AddRange(pageOrders)

            Catch ex As TaskCanceledException
                Traza.AppendText($"Timeout: {ex.Message}" & vbCrLf)
                MsgBox("Se agotó el tiempo de espera al conectar con la API.", vbExclamation, "Timeout")
            Catch ex As Exception
                Traza.AppendText($"Excepción: {ex.Message}" & vbCrLf & ex.StackTrace & vbCrLf)
                MsgBox($"Error de conexión: {ex.Message}", vbCritical, "Error")
            End Try
        End Using

        MaxPedido = pedidos.Select(Function(p) p("id").ToString()).FirstOrDefault()

        ' si existe el paramero LASTPEDIDO en la tabla PARAMETROS , lo leemos para usarlo como punto de partida
        ' en la actualización de pedidos. De esta forma, evitamos consultar el estado de todos los pedidos cada
        ' vez que se actualizan los pedidos y solo consultamos el estado de los pedidos que no han sido actualizados en la última ejecución.
        Dim cmdLast As New OleDb.OleDbCommand("SELECT VALPAR FROM PARAMETROS WHERE NOMPAR = 'LASTPEDIDO'", conn)
        Dim readerLast As OleDb.OleDbDataReader = cmdLast.ExecuteReader()
        If readerLast.Read() Then
            LastPedido = readerLast("VALPAR").ToString()
        End If
        readerLast.Close()

        ' Para cada pedido de la tabla Pedidos, si el estado es distinto de "FACTURADO" y distinto de "OLD"
        ' lo actualizamos consultando el estado del pedido
        ' en la API de WooCommerce.
        Dim cmd As New OleDb.OleDbCommand("SELECT Pedidos.IdPedido FROM Pedidos WHERE " &
            "(" &
            "Pedidos.[Estado] <> 'OLD' " &
            "And Pedidos.[Estado] <> 'FACTURADO' " &
            "And Pedidos.[Estado] <> 'ANULADO' " &
            ") " &
            "Or Pedidos.[Estado] Is NULL " &
            "ORDER BY Pedidos.IdPedido;", conn)
        Dim reader As OleDb.OleDbDataReader = cmd.ExecuteReader()
        Dim pedidosActualizar As New List(Of String)()
        Debug.Print(cmd.CommandText)
        OldTraza = Traza.Text
        While reader.Read()
            Traza.Text = OldTraza
            Traza.AppendText($"Pedido a actualizar: {reader("IdPedido").ToString()}" & vbCrLf)
            pedidosActualizar.Add(reader("IdPedido").ToString())
            LastPedido = reader("IdPedido").ToString()
        End While
        reader.Close()

        Traza.Text = OldTraza
        Traza.AppendText($"Total pedidos a actualizar: {pedidosActualizar.Count}" & vbCrLf)
        OldTraza = Traza.Text

        ' añadimos los nuevos
        Dim NuevosPedidos As Long = 0

        For NuevosPedidos = LastPedido + 1 To MaxPedido
            pedidosActualizar.Add(NuevosPedidos.ToString())
        Next

        Traza.Text = OldTraza
        Traza.AppendText($"Total pedidos a actualizar + nuevos: {pedidosActualizar.Count}" & vbCrLf)
        OldTraza = Traza.Text

        For Each idPedido As String In pedidosActualizar

            ConsumerKey = ClaveWoo
            ConsumerSecret = PassWoo
            ' Seguridad: TLS 1.2 (requerido por la mayoría de hosts)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            ' Cliente HTTP con descompresión automática
            Dim handlernew As New HttpClientHandler() With {
                .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            }

            Using client As New HttpClient(handlernew)
                client.Timeout = TimeSpan.FromSeconds(60)
                client.BaseAddress = New Uri(BaseUrl)
                ' Encabezados comunes
                client.DefaultRequestHeaders.Accept.Clear()
                client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)")
                Try
                    ' ✅ Credenciales como parámetros de URL (como funciona en el navegador)
                    Url = $"orders/{idPedido}?consumer_key={ConsumerKey}&consumer_secret={ConsumerSecret}"
                    Traza.Text = OldTraza
                    Traza.AppendText($"Solicitando actualización del pedido {idPedido}..." & NumPedidos + 1 & vbCrLf)
                    Traza.Refresh()
                    Dim resp As HttpResponseMessage = client.GetAsync(Url).Result
                    NumPedidos += 1
                    Traza.AppendText($"Status Code: {CInt(resp.StatusCode)} {resp.StatusCode} " & NumPedidos & " ")
                    Traza.Refresh()
                    If resp.IsSuccessStatusCode Then
                        Dim json As String = resp.Content.ReadAsStringAsync().Result
                        Dim pedido As Object = JsonConvert.DeserializeObject(Of Object)(json)
                        ProcesarPedido(pedido, conn)
                        LastPedido = idPedido
                    Else
                        If resp.StatusCode = HttpStatusCode.NotFound Then
                            Traza.AppendText($"Pedido {idPedido} no encontrado en WooCommerce." & vbCrLf)
                            ' Si el pedido no se encuentra, lo marcamos como "ANULADO" para no volver a consultarlo en futuras actualizaciones,
                            ' ya que puede ser un pedido antiguo que ya no existe en WooCommerce o un pedido que se ha eliminado.
                            Dim fechaActual As DateTime = DateTime.Now
                            Dim updateCmd As New OleDb.OleDbCommand($"UPDATE Pedidos SET Estado = 'ANULADO', Fecha = #{fechaActual:MM/dd/yyyy HH:mm:ss}# WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
                            updateCmd.Parameters.AddWithValue("@IdWeb", WebId)
                            updateCmd.Parameters.AddWithValue("@IdPedido", idPedido)
                            Debug.Print($"UPDATE Pedidos SET Estado = 'ANULADO', Fecha = #{fechaActual:MM/dd/yyyy HH:mm:ss}# WHERE IdWeb = '{WebId}' AND IdPedido = '{idPedido}'")
                            updateCmd.ExecuteNonQuery()
                            Traza.Text = OldTraza
                            Traza.AppendText($"Pedido {idPedido} marcado como ANULADO." & vbCrLf)
                            OldTraza = Traza.Text
                        Else
                            Dim bodyErr = resp.Content.ReadAsStringAsync().Result
                            Traza.Text = OldTraza
                            Traza.AppendText($"Error al actualizar pedido {idPedido}: {bodyErr}" & vbCrLf)
                            OldTraza = Traza.Text
                        End If
                        Traza.Refresh()
                    End If
                Catch ex As Exception
                    Traza.Text = OldTraza
                    Traza.AppendText($"Excepción al actualizar pedido {idPedido}: {ex.Message}" & vbCrLf & ex.StackTrace & vbCrLf)
                    OldTraza = Traza.Text
                    Traza.Refresh()
                End Try
            End Using

        Next


        ' Grabamos el último pedido actualizado en la tabla PARAMETROS para usarlo como punto de partida en la próxima actualización
        If Not String.IsNullOrEmpty(LastPedido) Then
            Dim cmdUpdate As New OleDb.OleDbCommand("UPDATE PARAMETROS SET VALPAR = ? WHERE NOMPAR = 'LASTPEDIDO'", conn)
            cmdUpdate.Parameters.AddWithValue("?", LastPedido)
            Dim rowsAffected As Integer = cmdUpdate.ExecuteNonQuery()

            ' Si no se actualizó ningún registro, insertamos uno nuevo
            If rowsAffected = 0 Then
                Dim cmdInsert As New OleDb.OleDbCommand("INSERT INTO PARAMETROS (NOMPAR, VALPAR) VALUES (?, ?)", conn)
                cmdInsert.Parameters.AddWithValue("?", "LASTPEDIDO")
                cmdInsert.Parameters.AddWithValue("?", LastPedido)
                cmdInsert.ExecuteNonQuery()
            End If
        End If

        ' Cerramos la conexión a la base de datos
        conn.Close()

        Traza.Text = OldTraza
        Traza.Text += "Pedidos actualizados correctamente" & vbCrLf
        Traza.Refresh()

    End Sub

    Private Sub BtnFacturar_Click(sender As Object, e As EventArgs) Handles BtnFacturar.Click

        Dim Facturar As Boolean = False

        Dim URL As String = WebFs
        Dim API As String = ClaveFs

        Dim Busqueda As String = ""
        Dim Cliente As String = ""

        Dim OldPais As String = ""
        Dim IdPais As String = ""

        Dim EmailCliente As String = ""
        Dim NombreCliente As String = ""
        Dim RazonSocialCliente As String = ""
        Dim TelefonoCliente As String = ""
        Dim Pais As String = ""
        Dim CifDniCliente As String = ""

        Dim NumTest As Integer = 0

        ' Abrimos la Base de datos para seleccionar los pedidos que se van a facturar
        Dim connString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & BaseDatos & "\Api_Woocommerce.mdb"
        Dim conn As New OleDb.OleDbConnection(connString)
        conn.Open()

        ' leemos los parametros TESTURL y TESTAPI de la tabla PARAMETROS para saber si estamos en modo test o no.
        ' En modo test, se facturan los pedidos en el entorno de pruebas de FacturaScripts, y en modo normal se facturan en el entorno de producción de FacturaScripts.
        Dim cmdTestUrl As New OleDb.OleDbCommand("SELECT VALPAR FROM PARAMETROS WHERE NOMPAR = 'TESTURL'", conn)
        If cmdTestUrl.ExecuteScalar() IsNot Nothing Then
            Dim testUrl As String = cmdTestUrl.ExecuteScalar().ToString()
            URL = testUrl
        End If

        Dim cmdTestApi As New OleDb.OleDbCommand("SELECT VALPAR FROM PARAMETROS WHERE NOMPAR = 'TESTAPI'", conn)
        If cmdTestApi.ExecuteScalar() IsNot Nothing Then
            Dim testApi As String = cmdTestApi.ExecuteScalar().ToString()
            API = testApi
        End If

        Traza.Text = ""
        Traza.AppendText("Facturando pedidos..." & vbCrLf)
        Traza.Refresh()

        ' Seleccionamos los pedidos con estado nulo o vacio para facturar ordenados por idpedido
        Dim cmd As New OleDb.OleDbCommand("SELECT IdPedido FROM Pedidos WHERE (Estado Is Null Or Estado = '') AND IdWeb = @IdWeb ORDER BY IdPedido", conn)
        cmd.Parameters.AddWithValue("@IdWeb", WebId)

        Dim pedidosAFacturar As New List(Of String)()

        Using reader As OleDb.OleDbDataReader = cmd.ExecuteReader()
            While reader.Read()
                pedidosAFacturar.Add(reader("IdPedido").ToString())
            End While
        End Using

        ' para cada pedido a facturar leemos el pedido de WooCommerce
        For Each idPedido As String In pedidosAFacturar
            'Traza.AppendText($"Facturando pedido {idPedido}..." & vbCrLf)
            'Traza.Refresh()

            Dim pedido As Object = Nothing
            pedido = LeerPedidoWoo(idPedido)

            Debug.Print($"Pedido leído para facturar: ID {pedido("id").ToString()}, Status {pedido("status").ToString()}")
            Debug.Print($"Pedido completo: {JsonConvert.SerializeObject(pedido)}")

            Debug.Print("")

            Facturar = True
            If pedido("status").ToString() = "auto-draft" Then Facturar = False
            If pedido("status").ToString() = "checkout-draft" Then Facturar = False
            If pedido("status").ToString() = "pending" Then Facturar = False
            If pedido("status").ToString() = "esperando-tu-pago" Then Facturar = False

            If Not Facturar Then
                Traza.AppendText($"Pedido {idPedido} está en estado {pedido("status").ToString()}, no se puede facturar." & vbCrLf)
                Traza.Refresh()
                Continue For
            End If

            Traza.AppendText($"Pedido {idPedido} facturable.")
            Traza.AppendText(pedido("billing")("email") & vbCrLf)
            'Traza.AppendText(pedido("billing")("first_name") & " " & pedido("billing")("last_name") & vbCrLf)

            ' Construir la URL con el email correctamente codificado
            EmailCliente = pedido("billing")("email").ToString()
            Busqueda = URL.TrimEnd("/"c) & "/api/3/clientes?filter[email]=" & Uri.EscapeDataString(EmailCliente)

            Traza.AppendText("Buscando cliente en FacturaScripts con email: " & EmailCliente & vbCrLf)
            Traza.Refresh()

            Cliente = LeerApi(Busqueda, API)

            Dim CodCliente As String = ""

            If Cliente = "[]" Then

                Debug.Print($"Cliente con email {pedido("billing")("email").ToString()} no encontrado en FacturaScripts, se creará un nuevo cliente al facturar el pedido.")

                NombreCliente = pedido("billing")("first_name").ToString() & " " & pedido("billing")("last_name").ToString()
                EmailCliente = pedido("billing")("email").ToString()
                TelefonoCliente = pedido("billing")("phone").ToString()
                ' si company no esta en blanco RazonSocialCliente es company, si no, es el nombre del cliente
                RazonSocialCliente = If(String.IsNullOrEmpty(pedido("billing")("company").ToString()), pedido("billing")("last_name").ToString() & " " & pedido("billing")("first_name").ToString(), pedido("billing")("company").ToString())
                Pais = pedido("billing")("country").ToString()

                ' Extraer _billing_nif de meta_data
                Dim metaData As JArray = pedido("meta_data")
                CifDniCliente = ""

                For Each meta As JObject In metaData
                    If meta("key").ToString() = "_billing_nif" Then
                        CifDniCliente = meta("value").ToString()
                        Exit For
                    End If
                Next

                If OldPais <> Pais Then
                    OldPais = Pais

                    ' Buscamos Pais en la tabla de FacturaScripts.
                    ' Para esto, usamos el campo Pais del pedido que contiene el codigo de pais en formato ISO2 (por ejemplo, ES para España,
                    ' MX para México, etc.) y lo comparamos con el campo CodPaisISO2 de la tabla Paises.
                    ' Si encontramos una coincidencia, usamos el campo CodPais de la tabla Paises como codigo de pais
                    ' para crear el cliente en FacturaScripts. Si no encontramos una coincidencia, dejamos el codigo de pais con el valor actual.
                    Dim BusquedaPais = URL.TrimEnd("/"c) & "/api/3/pais?filter[codiso]=" & Pais
                    Dim PaisJson As String = LeerApi(BusquedaPais, API)

                    If PaisJson <> "" Then
                        Dim Paises As JArray = JArray.Parse(PaisJson)
                        If Paises.Count > 0 Then
                            Pais = Paises(0)("codpais").ToString()
                            Debug.Print($"Pais encontrado en FacturaScripts: {Pais}")
                            IdPais = Pais
                        Else
                            Debug.Print($"Pais con codigo ISO2 {Pais} no encontrado en FacturaScripts, se usará el codigo de pais del pedido.")
                        End If
                    Else
                        Debug.Print($"Error al buscar pais con codigo ISO2 {Pais} en FacturaScripts, se usará el codigo de pais del pedido.")
                    End If
                Else
                    Pais = IdPais
                End If

                ' Creamos con los datos del pedido. Para esto, usamos el endpoint de creación de clientes de la API de FacturaScripts.
                ' con la rutina CrearApi, que recibe como parámetros la URL del endpoint de creación de clientes,
                ' el API key de FacturaScripts y el JSON con los datos del cliente a crear.
                Dim ClienteJson As String = JsonConvert.SerializeObject(New With {
                    .cifnif = CifDniCliente,
                    .nombre = NombreCliente,
                    .email = EmailCliente,
                    .telefono1 = TelefonoCliente,
                    .razonsocial = RazonSocialCliente,
                    .pais = Pais
                })
                Dim BusquedaCrearCliente = URL.TrimEnd("/"c) & "/api/3/clientes"
                Dim Resultado = CrearApi(BusquedaCrearCliente, ClienteJson, API)

                ' Extraer CodCliente del resultado de la creación
                If Not String.IsNullOrEmpty(Resultado) Then
                    Try
                        Dim ClienteCreado As JObject = JObject.Parse(Resultado)
                        CodCliente = ClienteCreado("data")("codcliente").ToString()
                        Traza.AppendText($"Cliente creado exitosamente con código: {CodCliente}" & vbCrLf)
                        Debug.Print($"Cliente creado en FacturaScripts: CodCliente {CodCliente}")
                    Catch ex As Exception
                        Traza.AppendText($"Error al parsear respuesta de creación de cliente: {ex.Message}" & vbCrLf)
                        Debug.Print($"Error al crear cliente en FacturaScripts: {Resultado}")
                    End Try
                Else
                    Traza.AppendText($"Error: No se recibió respuesta al crear el cliente" & vbCrLf)
                End If

            Else
                Dim Clientes As JArray = JArray.Parse(Cliente)
                If Clientes.Count > 0 Then
                    CodCliente = Clientes(0)("codcliente").ToString()
                    CifDniCliente = Clientes(0)("cifnif").ToString()
                    NombreCliente = Clientes(0)("nombre").ToString()
                    RazonSocialCliente = Clientes(0)("razonsocial").ToString()

                    ' leemos con la API de FacturaScripts el codigo de pais del cliente con la tabla contactos
                    ' idcontactofact de clientes -> idcontacto de contactos

                    Dim DirContacto As String = URL.TrimEnd("/"c) & "/api/3/contactos/" & Clientes(0)("idcontactofact").ToString
                    Dim ContactoJson As String = LeerApi(DirContacto, API)
                    Dim Contacto As JObject = JObject.Parse(ContactoJson)

                    Pais = Contacto("codpais").ToString()

                    If OldPais <> Pais Then
                        OldPais = Pais

                        ' Buscamos Pais en la tabla de FacturaScripts.
                        ' Para esto, usamos el campo Pais del pedido que contiene el codigo de pais en formato ISO2 (por ejemplo, ES para España,
                        ' MX para México, etc.) y lo comparamos con el campo CodPaisISO2 de la tabla Paises.
                        ' Si encontramos una coincidencia, usamos el campo CodPais de la tabla Paises como codigo de pais
                        ' para crear el cliente en FacturaScripts. Si no encontramos una coincidencia, dejamos el codigo de pais con el valor actual.
                        Dim BusquedaPais = URL.TrimEnd("/"c) & "/api/3/pais?filter[codpais]=" & Pais
                        Dim PaisJson As String = LeerApi(BusquedaPais, API)

                        If PaisJson <> "" Then
                            Dim Paises As JArray = JArray.Parse(PaisJson)
                            If Paises.Count > 0 Then
                                Pais = Paises(0)("codpais").ToString()
                                IdPais = Pais
                            Else
                            End If
                        Else
                        End If
                    Else
                        Pais = IdPais
                    End If
                    Debug.Print($"Cliente encontrado en FacturaScripts: CodCliente {CodCliente}")
                Else
                    Debug.Print($"Error al parsear cliente encontrado en FacturaScripts: {Cliente}")
                End If
            End If

            Traza.AppendText($"Cliente para pedido {idPedido}: {CodCliente} ({EmailCliente})" & vbCrLf)
            Traza.Refresh()

            Dim statusPedido As String = pedido("status").ToString()
            Dim datePedido As DateTime = DateTime.Parse(pedido("date_created").ToString())

            Dim totalPedido As String = pedido("total").ToString()
            Dim totalTaxPedido As String = pedido("total_tax").ToString()
            Dim shippingTotalPedido As String = pedido("shipping_total").ToString()
            Dim shippingTaxTotalPedido As String = pedido("shipping_tax").ToString()

            Dim direcccionFacturacion As String = pedido("billing")("address_1").ToString() & " " & pedido("billing")("address_2").ToString() & ", " & pedido("billing")("city").ToString() & ", " & pedido("billing")("state").ToString() & ", " & pedido("billing")("postcode").ToString()
            Dim CiudadPedido As String = pedido("billing")("city").ToString()
            Dim CodigoPostalPedido As String = pedido("billing")("postcode").ToString()
            Dim ProvinciaPedido As String = pedido("billing")("state").ToString()

            ' leemos el nombre de la provincia en la Api de Facturascripts con el campo ProvinciaPedido que contiene el codigo
            ' de la provincia en formato ISO y el campo Pais que contiene el codigo de pais en formato ISO2. 

            Dim BusquedaProvincia As String = URL.TrimEnd("/"c) & "/api/3/provincias?filter[codisoprov]=" & ProvinciaPedido & "&filter[codpais]=" & Pais
            Dim NomProvincia As String = LeerApi(BusquedaProvincia, API)
            If NomProvincia <> "" Then
                Dim Provincias As JArray = JArray.Parse(NomProvincia)
                If Provincias.Count > 0 Then
                    ProvinciaPedido = Provincias(0)("provincia").ToString()
                Else
                End If
            Else
            End If

            Dim pagoPedido As String = pedido("payment_method").ToString()
            Dim TipoPagoPedido As String = pedido("payment_method_title").ToString()

            Dim fees As JArray = pedido("fee_lines")
            Dim lineas As JArray = pedido("line_items")
            Dim shippingLines As JArray = pedido("shipping_lines")

            Debug.Print(CodCliente & " Cliente " & CifDniCliente)
            Debug.Print(NombreCliente & " " & "NombreCliente")
            Debug.Print(RazonSocialCliente & " " & "RazonSocialCliente")
            Debug.Print(Pais & " " & "Pais")

            ' Para cada linea del pedido, obtenemos el producto o servicio, la cantidad, el precio unitario, el total y los impuestos,
            ' y los grabamos en un JSON LineasPedido para enviarlo a la API de creación de facturas de FacturaScripts.
            ' Ha de tener la siguiente estructura:
            ' [{"referencia":"gc-4795","cantidad":1,"descripcion":"Servicio de Grading PRO","pvpunitario":127.68595,"codimpuesto":"IVA21","iva":21}]

            Dim LineasPedido As New List(Of Object)()

            ' Procesar líneas de productos
            For Each linea As JObject In lineas
                Dim referencia As String = If(linea("sku") IsNot Nothing, linea("sku").ToString(), "")
                Dim cantidad As String = linea("quantity").ToString()
                Dim descripcion As String = linea("name").ToString()
                Dim pvpunitario As Decimal = Decimal.Parse(linea("price").ToString())

                ' verificamos si la linea en el campo meta_data tiene las claves
                ' Detalle cartas
                ' Tipo de servicio
                ' Si las tiene, añadimos esa información a la descripción de la linea.
                Dim metaData As JArray = linea("meta_data")

                For Each meta As JObject In metaData
                    If meta("key").ToString() = "Detalles cartas" Then
                        descripcion &= vbCrLf & "Detalles cartas" & vbCrLf & meta("value").ToString().Replace("<br>", vbCrLf).Replace("<br/>", vbCrLf).Replace("<BR>", vbCrLf)
                    End If
                    If meta("key").ToString() = "Tipo de servicio" Then
                        descripcion &= vbCrLf & "Tipo de servicio - " & meta("value").ToString()
                    End If
                Next

                LineasPedido.Add(New With {
                    .referencia = referencia,
                    .cantidad = cantidad,
                    .descripcion = descripcion,
                    .pvpunitario = pvpunitario,
                    .codimpuesto = "IVA21",
                    .iva = 21
                })
            Next

            ' Procesar líneas de envío
            For Each shippingLine As JObject In shippingLines
                Dim descripcion As String = shippingLine("method_title").ToString()
                Dim pvpunitario As String = shippingLine("total").ToString()

                LineasPedido.Add(New With {
                    .referencia = "ENVIO",
                    .cantidad = 1,
                    .descripcion = descripcion,
                    .pvpunitario = pvpunitario,
                    .codimpuesto = "IVA21",
                    .iva = 21
                })
            Next

            ' Procesar fees
            For Each fee As JObject In fees
                Dim descripcion As String = fee("name").ToString()
                Dim pvpunitario As String = fee("amount").ToString()

                LineasPedido.Add(New With {
                    .referencia = "SEGURO",
                    .cantidad = 1,
                    .descripcion = descripcion,
                    .pvpunitario = pvpunitario,
                    .codimpuesto = "IVA21",
                    .iva = 21
                })
            Next

            ' Serializar a JSON
            Dim LineasPedidoJson As String = JsonConvert.SerializeObject(LineasPedido)

            Debug.Print("Líneas del pedido " & idPedido & ": " & LineasPedidoJson)
            Traza.AppendText($"Líneas procesadas: {LineasPedido.Count}" & vbCrLf)
            Traza.Refresh()

            ' preparamos el JSON de la factura usando estos campos
            ' CodCliente -> CodCliente 
            ' cifnif -> CifDniCliente 
            ' NombreCliente -> RazonSocialCliente 
            ' direccion -> direcccionFacturacion
            ' ciudad -> CiudadPedido
            ' provincia -> ProvinciaPedido
            ' codpostal -> CodigoPostalPedido
            ' codpais -> Pais
            ' fecha -> datePedido
            ' hora -> datePedido
            ' numero2 -> idPedido
            ' total -> totalPedido
            ' totaliva -> totalTaxPedido

            Dim pagada As String = "0"
            Dim formaPago As String = "Otro"

            If pedido("payment_method").ToString() = "redsys" Then
                formaPago = "TARJETA"
                pagada = "1"
            End If

            If pedido("payment_method").ToString() = "tarjeta" Then formaPago = "TARJTIENDA"
            If pedido("payment_method").ToString() = "pos_cash" Then formaPago = "CONT"
            If pedido("payment_method").ToString() = "cod" Then formaPago = "CONT"
            If pedido("payment_method").ToString() = "efectivo" Then formaPago = "CONT"
            If pedido("payment_method").ToString() = "bacs" Then formaPago = "TRANS"
            If pedido("payment_method").ToString() = "transferencia" Then formaPago = "TRANS"

            Dim FacturaJson As String = JsonConvert.SerializeObject(New With {
                .codcliente = CodCliente,
                .cifnif = CifDniCliente,
                .nombre = RazonSocialCliente,
                .direccion = direcccionFacturacion,
                .ciudad = CiudadPedido,
                .provincia = ProvinciaPedido,
                .codpostal = CodigoPostalPedido,
                .codpais = Pais,
                .fechadevengo = datePedido.ToString("yyyy-MM-dd"),
                .fecha = datePedido.ToString("yyyy-MM-dd"),
                .hora = datePedido.ToString("HH:mm:ss"),
                .numero2 = idPedido,
                .codpago = formaPago,
                .pagada = pagada,
                .lineas = LineasPedidoJson
            })

            ' grabamos con la API de FacturaScripts la factura.
            ' Para esto, usamos el endpoint de creación de facturas de la API de FacturaScripts.

            Dim resultadoFactura = CrearApi(URL.TrimEnd("/"c) & "/api/3/crearFacturaCliente", FacturaJson, API)

            Debug.Print($"Resultado al crear factura para pedido {idPedido}: {resultadoFactura}")

            ' Si la factura se ha creado correctamente, devuelve un JSON con el id de la factura creada, por ejemplo: {"doc":{"apartado":"","cifnif":"03942072",...,
            ' si se ha creado correctamente la factura, actualizamos el estado del pedido a "FACTURADO" y la fecha de facturación en la base de datos.
            ' Si no se ha creado correctamente, dejamos el estado del pedido sin actualizar para intentar facturarlo en la próxima ejecución.


            ' Verificar si la factura se creó correctamente
            If Not String.IsNullOrEmpty(resultadoFactura) Then
                Try
                    Dim facturaCreada As JObject = JObject.Parse(resultadoFactura)

                    ' Comprobar si existe el nodo "doc" que indica éxito
                    If facturaCreada("doc") IsNot Nothing Then
                        ' Factura creada exitosamente - actualizar estado del pedido
                        Dim fechaActual As DateTime = DateTime.Now
                        Dim updateCmd As New OleDb.OleDbCommand($"UPDATE Pedidos SET Estado = 'FACTURADO', Fecha = #{fechaActual:MM/dd/yyyy HH:mm:ss}# WHERE IdWeb = @IdWeb AND IdPedido = @IdPedido", conn)
                        updateCmd.Parameters.AddWithValue("@IdWeb", WebId)
                        updateCmd.Parameters.AddWithValue("@IdPedido", idPedido)
                        updateCmd.ExecuteNonQuery()

                        Traza.AppendText($"✓ Pedido {idPedido} facturado correctamente" & vbCrLf)
                        Debug.Print($"Pedido {idPedido} marcado como FACTURADO")
                    Else
                        ' Error en la creación - mantener estado NULL para reintentar
                        Traza.AppendText($"✗ Error al facturar pedido {idPedido}: {resultadoFactura}" & vbCrLf)
                        Debug.Print($"Error en facturación pedido {idPedido}: {resultadoFactura}")
                    End If

                Catch ex As Exception
                    ' Error al parsear respuesta - mantener estado NULL
                    Traza.AppendText($"✗ Error al procesar respuesta para pedido {idPedido}: {ex.Message}" & vbCrLf)
                    Debug.Print($"Excepción al procesar factura pedido {idPedido}: {ex.ToString()}")
                End Try
            Else
                ' No hay respuesta - mantener estado NULL
                Traza.AppendText($"✗ Sin respuesta al crear factura para pedido {idPedido}" & vbCrLf)
            End If
            Traza.Refresh()

            ' pausamos por 15 segundos entre cada factura para no saturar el servidor de FacturaScripts y para evitar problemas de timeout o de bloqueo por parte del servidor de FacturaScripts.
            Threading.Thread.Sleep(5000)


            NumTest += 1
            If NumTest >= 25 Then Exit For

        Next




        ' cierre de la conexión a la base de datos
        conn.Close()

        Traza.Text += "Pedidos facturados correctamente" & vbCrLf
        Traza.Refresh()

    End Sub

    Public Function LeerPedidoWoo(idPedido As String) As Object
        Dim pedido As Object = Nothing
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
            Try
                ' ✅ Credenciales como parámetros de URL (como funciona en el navegador)
                Dim url = $"orders/{idPedido}?consumer_key={ConsumerKey}&consumer_secret={ConsumerSecret}"
                'Traza.AppendText($"Solicitando pedido {idPedido} para facturar..." & vbCrLf)
                'Traza.Refresh()
                Dim resp As HttpResponseMessage = client.GetAsync(url).Result
                'Traza.AppendText($"Status Code: {CInt(resp.StatusCode)} {resp.StatusCode}" & vbCrLf)
                'Traza.Refresh()
                If resp.IsSuccessStatusCode Then
                    Dim json As String = resp.Content.ReadAsStringAsync().Result
                    pedido = JsonConvert.DeserializeObject(Of Object)(json)
                Else
                    Dim bodyErr = resp.Content.ReadAsStringAsync().Result
                    Traza.AppendText($"Error al leer pedido {idPedido} para facturar: {bodyErr}" & vbCrLf)
                    Traza.Refresh()
                End If
            Catch ex As Exception
                Traza.AppendText($"Excepción al leer pedido {idPedido} para facturar: {ex.Message}" & vbCrLf & ex.StackTrace & vbCrLf)
                Traza.Refresh()
            End Try
        End Using
        Return pedido
    End Function

End Class
