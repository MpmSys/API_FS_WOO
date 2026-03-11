Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
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

    Public Function CrearApi(RequestURL As String, StrData As String) As String

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
        WebRequest.Method = CRUDMethod.Create
        WebRequest.Headers.Add(ApiLoc)
        ' Convertir StrData JSON a application/x-www-form-urlencoded si es necesario
        Dim postData As String = ""
        If Not String.IsNullOrWhiteSpace(StrData) Then
            Dim parsedOk As Boolean = False
            Dim normalized As String = NormalizeJsonString(StrData)

            Try
                ' Intentar parsear la cadena normalizada como JSON
                Dim jo As JObject = JObject.Parse(normalized)
                Dim parts As New System.Collections.Generic.List(Of String)
                For Each prop As JProperty In jo.Properties()
                    Dim name As String = prop.Name
                    Dim valueToken As JToken = prop.Value
                    Dim value As String

                    ' Si el valor es un objeto o array, serializarlo; si es booleano o número, usar su representación
                    Select Case valueToken.Type
                        Case JTokenType.Boolean
                            value = If(CBool(valueToken), "true", "false")
                        Case JTokenType.Null
                            value = ""
                        Case JTokenType.String, JTokenType.Integer, JTokenType.Float
                            value = valueToken.ToString()
                        Case Else
                            value = valueToken.ToString(Newtonsoft.Json.Formatting.None)
                    End Select

                    parts.Add(Uri.EscapeDataString(name) & "=" & Uri.EscapeDataString(value))
                Next
                postData = String.Join("&", parts)
                parsedOk = True
            Catch ex As Exception
                ' No es JSON válido incluso tras normalizar: caeremos al fallback
                Debug.Print("Normalizado no parseable: " & ex.Message)
            End Try

            If Not parsedOk Then
                ' Fallback: usar la cadena original (o la normalizada) tal cual, para casos form-urlencoded
                ' Preferimos la cadena normalizada para corregir True/False y comas finales si procede,
                ' pero si la cadena parece claramente form-url-encoded la dejaremos como estaba.
                If StrData.Contains("=") AndAlso Not normalized.Contains("{") Then
                    postData = StrData
                Else
                    postData = normalized
                End If
            End If
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
            Traza.Text += "CrearAPI FacturaScript " & vbCrLf & RequestURL & vbCrLf & ex.ToString & vbCrLf
            StrDataRes = ""
        End Try

        Debug.Print(StrDataRes)
        CrearApi = StrDataRes

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
        If status = "cancelled" Then
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

    Private Sub BtnActualizar_Click(sender As Object, e As EventArgs) Handles BtnActualizar.Click

        Dim LastPedido As String = ""
        Dim NumPedidos As Integer = 0

        ' Abrimos la Base de datos para mantener la tabla Stock
        Dim connString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & BaseDatos & "\Api_Woocommerce.mdb"
        Dim conn As New OleDb.OleDbConnection(connString)
        conn.Open()

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

        For Each idPedido As String In pedidosActualizar

            ' Configuración de la API de WooCommerce
            Dim BaseUrl As String = WebWoo.TrimEnd("/"c) & "/wp-json/wc/v3/"
            Dim ConsumerKey As String
            Dim ConsumerSecret As String
            ConsumerKey = ClaveWoo
            ConsumerSecret = PassWoo
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
                    Traza.Text = OldTraza
                    Traza.AppendText($"Solicitando actualización del pedido {idPedido}..." & NumPedidos + 1 & vbCrLf)
                    Traza.Refresh()
                    Dim resp As HttpResponseMessage = client.GetAsync(url).Result
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
End Class
