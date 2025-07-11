using Transbank.POSAutoservicio;
using Transbank.Responses.CommonResponses;
using Transbank.Responses.AutoservicioResponse;
using System.Drawing.Printing;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SalidaAutomaticaQR.Models;

public enum ReceiptType
{

    Sale,

    Close

}

class Program
{

    static async Task Main(string[] args)
    {

        #region Seleccionar impresora
        List<string> printers = new List<string>();

        foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
        {

            printers.Add(installedPrinter);

        }

        if (printers.Count == 0)
        {

            Console.WriteLine("No se encontraron impresoras instaladas.");

            return;

        }

        Console.WriteLine("Impresoras disponibles:");

        Console.WriteLine();

        for (int i = 0; i < printers.Count; i++)
        {

            Console.WriteLine($"{i + 1}: {printers[i]}");

        }

        Console.WriteLine($"\nSeleccione el número de la impresora a utilizar (1 - {printers.Count}):");

        int printerIndex;

        Console.WriteLine();

        string printerInput = Console.ReadLine();

        Console.WriteLine();

        while (!int.TryParse(printerInput, out printerIndex) || printerIndex < 1 || printerIndex > printers.Count)
        {

            Console.WriteLine("Selección inválida. Intente nuevamente:");

            printerInput = Console.ReadLine();

        }

        string selectedPrinter = printers[printerIndex - 1];

        Printer printer = new Printer(selectedPrinter);
        #endregion

        #region Seleccionar puerto (Recomendado COM9 "PAX")
        List<string> ports = POSAutoservicio.Instance.ListPorts();

        if (ports == null || ports.Count == 0)
        {

            Console.WriteLine("No se encontraron puertos disponibles del POS.");

            return;

        }

        Console.WriteLine("Puertos disponibles:");

        Console.WriteLine();

        for (int i = 0; i < ports.Count; i++)
        {

            Console.WriteLine($"{i + 1}: {ports[i]}");

        }

        Console.WriteLine($"\nSeleccione el número del puerto a utilizar (1 - {ports.Count}):");

        int portIndex;

        Console.WriteLine();

        string portInput = Console.ReadLine();

        Console.WriteLine();

        while (!int.TryParse(portInput, out portIndex) || portIndex < 1 || portIndex > ports.Count)
        {

            Console.WriteLine("Selección inválida. Intente nuevamente:");

            portInput = Console.ReadLine();

        }

        string selectedPort = ports[portIndex - 1];
        #endregion

        #region Seleccionar base de datos

        // Configuración
        var config = new ConfigurationBuilder()

            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

            .Build();

        var conexiones = config.GetSection("Conexiones").GetChildren().ToDictionary(x => x.Key, x => x.Value);

        string[] opciones = conexiones.Keys.ToArray();

        string claveSeleccionada = "";

        string cadenaConexion = "";

        bool conexionExitosa = false;

        while (!conexionExitosa)
        {

            Console.WriteLine("Bases de datos disponibles:");

            Console.WriteLine();

            for (int i = 0; i < opciones.Length; i++)
            {

                Console.WriteLine($"{i + 1}. {FormatoNombre(opciones[i])}");

            }

            int seleccion = 0;

            while (seleccion < 1 || seleccion > opciones.Length)
            {

                Console.Write("\nIngrese una opción (1 - 6): ");

                int.TryParse(Console.ReadLine(), out seleccion);

                if (seleccion < 1 || seleccion > opciones.Length)
                {

                    Console.WriteLine($"\nOpción inválida. Debe ser un número entre 1 y {opciones.Length}");

                }

            }

            claveSeleccionada = opciones[seleccion - 1];

            cadenaConexion = conexiones[claveSeleccionada];

            Console.WriteLine($"\n⏳ Probando conexión a: {FormatoNombre(claveSeleccionada)}...");

            await ActualizarConexionEnJson(claveSeleccionada, conexiones);

            // Probar la conexión
            conexionExitosa = await ProbarConexion();

            if (conexionExitosa)
            {

                Console.WriteLine($"\n[MAIN THREAD] ✅ Conexión exitosa!");

                break;

            }

            else
            {

                Console.WriteLine($"\nError: No se pudo establecer conexión con {FormatoNombre(claveSeleccionada)}");

                Console.WriteLine("\nPor favor, seleccione otra opción.");

            }

        }

        Console.WriteLine($"\nConexión establecida: {FormatoNombre(claveSeleccionada)}");
        
        #endregion

        try
        {

            bool @continue = true;

            while (@continue)
            {

                POSAutoservicio.Instance.OpenPort(selectedPort);

                Console.WriteLine("\nSeleccione la acción a realizar:");

                Console.WriteLine("1. Venta");

                Console.WriteLine("2. Cierre");

                Console.WriteLine("3. Salir");

                Console.Write("Ingrese el número de la opción: ");

                Console.WriteLine("Puertos disponibles:");

                int optionIndex;

                string optionInput = Console.ReadLine();

                if (!int.TryParse(optionInput, out optionIndex) || optionIndex < 1 || optionIndex > 3)
                {

                    Console.WriteLine("Opción inválida. Intente nuevamente.");

                    continue;

                }

                switch (optionIndex)
                {

                    case 1: // Venta

                        Console.WriteLine("Ingrese el monto a solicitar:");

                        string input = Console.ReadLine();

                        int amount;

                        while (!int.TryParse(input, out amount) || amount < 50)
                        {

                            Console.WriteLine("Monto inválido. Debe ser un número mayor o igual a 50. Intente nuevamente:");

                            input = Console.ReadLine();

                        }

                        Console.WriteLine("Ingrese el ticket (máx 20 caracteres):");

                        string ticket = Console.ReadLine();

                        if (ticket.Length > 20)
                        {

                            ticket = ticket.Substring(0, 20);

                            Console.WriteLine("El ticket fue truncado a 20 caracteres.");

                        }

                        POSAutoservicio.Instance.IntermediateResponseChange += NewIntermediateMessageReceived;

                        var saleResponse = POSAutoservicio.Instance.Sale(amount, ticket, true, false);

                        saleResponse.Wait();

                        POSAutoservicio.Instance.IntermediateResponseChange -= NewIntermediateMessageReceived;

                        Console.WriteLine("Venta realizada exitosamente:");

                        await printer.PrintVoucher(saleResponse.Result.Response, ReceiptType.Sale);

                        break;

                    case 2: // Cierre

                        POSAutoservicio.Instance.IntermediateResponseChange += NewIntermediateMessageReceived;

                        var closeResponse = POSAutoservicio.Instance.Close(true);

                        closeResponse.Wait();

                        POSAutoservicio.Instance.IntermediateResponseChange -= NewIntermediateMessageReceived;

                        Console.WriteLine("Cierre realizado:");

                        await printer.PrintVoucher(closeResponse.Result.Response, ReceiptType.Close);

                        break;

                    case 3: // Salir

                        @continue = false;

                        break;

                }

                POSAutoservicio.Instance.ClosePort();

            }

        }

        catch (Exception ex)
        {

            POSAutoservicio.Instance.ClosePort();

            Console.WriteLine("Error al realizar la venta: " + ex.Message);

        }

    }

    static string FormatoNombre(string clave)
    {

        return clave

        .Replace("Estacionamiento", "Estacionamiento ")

        .Replace("Desarrollo", " (Desarrollo)")

        .Replace("OrellaUno", "Orella Uno")

        .Replace("OrellaDos", "Orella Dos")

        .Replace("21DeMayo", "21 de Mayo")

        .Trim();

    }

    static async Task ActualizarConexionEnJson(string claveSeleccionada, Dictionary<string, string> conexiones)
    {

        var nuevoJson = new
        {

            ConexionSeleccionada = claveSeleccionada,

            Conexiones = conexiones

        };

        await File.WriteAllTextAsync("appsettings.json", JsonSerializer.Serialize(nuevoJson, new JsonSerializerOptions { WriteIndented = true }));

    }

    static async Task<bool> ProbarConexion()
    {

        try
        {

            using (var context = new EstacionamientoContext())
            {

                var estadoConexion = await context.Database.CanConnectAsync();

                return estadoConexion;

            }

        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error de conexión: {ex.Message}");

            return false;

        }

    }

    private static void NewIntermediateMessageReceived(object sender, IntermediateResponse e)
    {

        if (e == null)
        {

            Console.WriteLine("Mensaje intermedio recibido: sin datos.");

            return;

        }

        // Suponiendo que IntermediateResponse tiene propiedades como Message, Code, etc.
        Console.WriteLine($"Mensaje intermedio recibido:");

        Console.WriteLine($"Código: {e.ResponseCode}");

        Console.WriteLine($"Mensaje: {e.ResponseMessage}");

    }

}
 
public class Printer
{

    private string _printerName;

    public Printer(string printerName = null)
    {

        _printerName = printerName ?? "EPSON TM-m30 Receipt"; // Nombre por defecto

    }

    public async Task PrintVoucher(string voucher ,ReceiptType receiptType)
    {

        try
        {

            string receiptText;

            switch(receiptType)
            {

                case ReceiptType.Sale:


                    receiptText = FormatTransbankReceipt(voucher);

                    Console.WriteLine("Imprimiendo comprobante de venta...");

                    break;

                case ReceiptType.Close:

                    receiptText = FormatTransbankClose(voucher);

                    Console.WriteLine("Imprimiendo comprobante de cierre...");

                    break;

                default:

                    throw new ArgumentException("Tipo de recibo no soportado");

            }

            PrintDocument printDocument = new PrintDocument();

            printDocument.PrinterSettings.PrinterName = _printerName;

            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 600);

            printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            printDocument.PrintPage += (sender, e) =>
            {

                if (receiptType == ReceiptType.Sale)

                    LayoutSaleContent(e, receiptText);

                if (receiptType == ReceiptType.Close)

                    LayoutCloseContent(e, receiptText);

            };

            printDocument.Print();

            printDocument.Dispose();

            Console.WriteLine("Comprobante impreso exitosamente");

        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error al imprimir: {ex.Message}");

        }

    }

    // Método para imprimir el contenido del comprobante
    private void LayoutSaleContent(PrintPageEventArgs e, string receiptText)
    {
        // Fuentes para diferentes secciones
        Font titleFont = new Font("Arial", 10, FontStyle.Bold);
        Font normalFont = new Font("Arial", 8, FontStyle.Regular);
        Font smallFont = new Font("Arial", 7, FontStyle.Regular);

        SolidBrush brush = new SolidBrush(Color.Black);

        string[] lines = receiptText.Split('\n');
        float yPos = 10;
        float lineHeight = normalFont.GetHeight(e.Graphics);
        float pageWidth = e.PageBounds.Width - 20; // Margen de 10 a cada lado

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                yPos += lineHeight * 0.5f; // Línea en blanco más pequeña
                continue;
            }

            Font currentFont = normalFont;
            StringFormat format = new StringFormat();

            // Determinar formato según el contenido
            if (trimmedLine.Contains("COMPROBANTE DE VENTA") ||
                trimmedLine.Contains("TARJETA DE CREDITO") ||
                trimmedLine.Contains("INTEGRACIONES TRANSBANK") ||
                trimmedLine.Contains("TRANSBANK S.A.") ||
                trimmedLine.Contains("GRACIAS POR SU COMPRA"))
            {
                currentFont = titleFont;
                format.Alignment = StringAlignment.Center;
            }
            else if (trimmedLine.Contains("NUMERO DE TARJETA") ||
                     trimmedLine.Contains("VISA") ||
                     trimmedLine.Contains("Santiago"))
            {
                format.Alignment = StringAlignment.Center;
            }
            else if (trimmedLine.Contains("FECHA") && trimmedLine.Contains("HORA"))
            {
                // Encabezado de fecha/hora
                currentFont = smallFont;
                format.Alignment = StringAlignment.Near;
            }
            else
            {
                format.Alignment = StringAlignment.Near;
            }

            // Dibujar la línea
            RectangleF rect = new RectangleF(10, yPos, pageWidth, lineHeight);
            e.Graphics.DrawString(trimmedLine, currentFont, brush, rect, format);

            yPos += currentFont.GetHeight(e.Graphics);
        }

        // Limpiar recursos
        titleFont.Dispose();
        normalFont.Dispose();
        smallFont.Dispose();
        brush.Dispose();
    }

    // Método para imprimir el contenido del cierre
    private void LayoutCloseContent(PrintPageEventArgs e, string closeText)
    {
        Font titleFont = new Font("Arial", 10, FontStyle.Bold);
        Font normalFont = new Font("Arial", 8, FontStyle.Regular);
        Font smallFont = new Font("Arial", 7, FontStyle.Regular);

        SolidBrush brush = new SolidBrush(Color.Black);

        string[] lines = closeText.Split('\n');
        float yPos = 10;
        float lineHeight = normalFont.GetHeight(e.Graphics);
        float pageWidth = e.PageBounds.Width - 20; // Margen de 10 a cada lado

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                yPos += lineHeight * 0.5f;
                continue;
            }

            Font currentFont = normalFont;
            StringFormat format = new StringFormat();

            // Formato especial para encabezados y totales
            if (trimmedLine.Contains("REPORTE DEL CIERRE DEL TERMINAL") ||
                trimmedLine.Contains("INTEGRACIONES TRANSBANK") ||
                trimmedLine.Contains("TRANSBANK S.A.") ||
                trimmedLine.Contains("GRACIAS POR SU COMPRA") ||
                trimmedLine.Contains("TOTAL CAPTURAS"))
            {
                currentFont = titleFont;
                format.Alignment = StringAlignment.Center;
            }
            else if (trimmedLine.Contains("FECHA") && trimmedLine.Contains("HORA"))
            {
                currentFont = smallFont;
                format.Alignment = StringAlignment.Near;
            }
            else if (trimmedLine.Contains("----------------------------------------"))
            {
                format.Alignment = StringAlignment.Center;
            }
            else
            {
                format.Alignment = StringAlignment.Near;
            }

            RectangleF rect = new RectangleF(10, yPos, pageWidth, lineHeight);
            e.Graphics.DrawString(trimmedLine, currentFont, brush, rect, format);

            yPos += currentFont.GetHeight(e.Graphics);
        }

        // Limpiar recursos
        titleFont.Dispose();
        normalFont.Dispose();
        smallFont.Dispose();
        brush.Dispose();
    }

    // Método para formatear el comprobante de Transbank
    public string FormatTransbankReceipt(string rawResponse)
    {
        StringBuilder formattedReceipt = new StringBuilder();

        // Extraer información específica del response
        string[] data = rawResponse.Split('|');

        if (data.Length >= 15)
        {
            formattedReceipt.AppendLine("COMPROBANTE DE VENTA");
            formattedReceipt.AppendLine("TARJETA DE CREDITO");
            formattedReceipt.AppendLine("");
            formattedReceipt.AppendLine("INTEGRACIONES TRANSBANK");
            formattedReceipt.AppendLine("TRANSBANK S.A.");
            formattedReceipt.AppendLine("ISIDORA GOYENECHEA 3520");
            formattedReceipt.AppendLine("111111111");
            formattedReceipt.AppendLine("Santiago");
            formattedReceipt.AppendLine("");
            formattedReceipt.AppendLine($"{data[2]}-M252L3");
            formattedReceipt.AppendLine("");
            formattedReceipt.AppendLine("FECHA         HORA      TERMINAL");

            // Formatear fecha y hora
            string fecha = data[13]; // 10072025
            string hora = data[14];   // 115249
            string fechaFormateada = $"{fecha.Substring(0, 2)}/{fecha.Substring(2, 2)}/{fecha.Substring(4, 2)}";
            string horaFormateada = $"{hora.Substring(0, 2)}:{hora.Substring(2, 2)}:{hora.Substring(4, 2)}";

            formattedReceipt.AppendLine($"{fechaFormateada}        {horaFormateada}        {data[3]}");
            formattedReceipt.AppendLine("");
            formattedReceipt.AppendLine("NUMERO DE TARJETA");
            formattedReceipt.AppendLine($"B-CR************{data[7]}");
            formattedReceipt.AppendLine("VISA");
            formattedReceipt.AppendLine("");

            // Formatear monto
            string monto = data[6];
            formattedReceipt.AppendLine($"TOTAL:                    $ {monto}");
            formattedReceipt.AppendLine($"NUMERO DE BOLETA:         {data[4]}");
            formattedReceipt.AppendLine($"NUMERO DE OPERACION:      {data[8].PadLeft(6, '0')}");
            formattedReceipt.AppendLine($"CODIGO DE AUTORIZACION:   {data[5]}");
            formattedReceipt.AppendLine("");
            formattedReceipt.AppendLine("GRACIAS POR SU COMPRA");
            formattedReceipt.AppendLine("ACEPTO PAGAR SEGUN CONTRATO CON EMISOR");
            formattedReceipt.AppendLine("");
        }

        return formattedReceipt.ToString();
    }

    // Método para formatear el cierre de Transbank
    public string FormatTransbankClose(string rawResponse)
    {
        // Separar por pipe
        string[] data = rawResponse.Split('|');
        if (data.Length < 5)
            return rawResponse;

        StringBuilder sb = new StringBuilder();

        // Encabezado
        sb.AppendLine("REPORTE DEL CIERRE DEL TERMINAL");
        sb.AppendLine("INTEGRACIONES TRANSBANK");
        sb.AppendLine("TRANSBANK S.A.");
        sb.AppendLine("ISIDORA GOYENECHEA 3520");
        sb.AppendLine("111111111");
        sb.AppendLine("Santiago");
        sb.AppendLine($"{data[2]}-M252L3");
        sb.AppendLine();

        // Buscar y formatear fecha, hora y terminal
        string cuerpo = data[4];
        int idxFecha = cuerpo.IndexOf("FECHA");
        int idxNum = cuerpo.IndexOf("NUMERO");
        if (idxFecha > 0 && idxNum > idxFecha)
        {
            string header = cuerpo.Substring(0, idxNum);
            string totales = cuerpo.Substring(idxNum);

            // Extraer fecha, hora y terminal
            int idxTerminal = header.IndexOf("TERMINAL");
            if (idxTerminal > 0)
            {
                string fechaHoraTerminal = header.Substring(idxTerminal + 8).Trim();
                sb.AppendLine("FECHA        HORA        TERMINAL");
                sb.AppendLine(fechaHoraTerminal);
                sb.AppendLine();
            }

            // Extraer totales por tarjeta y total capturas
            string[] lineas = totales.Split(new[] { "----------------------------------------" }, StringSplitOptions.None);
            if (lineas.Length > 1)
            {
                string detalle = lineas[0];
                string total = lineas[1];

                // Detalle por tarjeta
                var tarjetas = detalle.Split(new[] { "VISA", "MASTERCARD" }, StringSplitOptions.RemoveEmptyEntries);
                if (detalle.Contains("MASTERCARD"))
                {
                    int idx = detalle.IndexOf("MASTERCARD");
                    string mc = "MASTERCARD" + detalle.Substring(idx + "MASTERCARD".Length).Split('V')[0];
                    sb.AppendLine(mc.Trim());
                }
                if (detalle.Contains("VISA"))
                {
                    int idx = detalle.IndexOf("VISA");
                    string visa = "VISA" + detalle.Substring(idx + "VISA".Length);
                    sb.AppendLine(visa.Trim());
                }

                sb.AppendLine("----------------------------------------");
                sb.AppendLine(total.Trim());
            }
            else
            {
                sb.AppendLine(totales.Trim());
            }
        }
        else
        {
            sb.AppendLine(cuerpo.Trim());
        }

        return sb.ToString();
    }

    // Método para verificar si la impresora está disponible
    public bool IsPrinterAvailable()
    {

        try
        {

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {

                if (printer.Equals(_printerName, StringComparison.OrdinalIgnoreCase))
                {

                    return true;

                }

            }

            return false;

        }

        catch
        {

            return false;

        }

    }

    // Método para listar impresoras disponibles
    public void ListAvailablePrinters()
    {
        Console.WriteLine("Impresoras disponibles:");
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            Console.WriteLine($"- {printer}");
        }
    }
}