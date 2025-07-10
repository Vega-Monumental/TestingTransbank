using Transbank.POSAutoservicio;
using Transbank.Responses.CommonResponses;
using Transbank.Responses.AutoservicioResponse;
using System.Drawing.Printing;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Text;

class Program
{

    static async Task Main(string[] args)
    {

        List<string> ports = POSAutoservicio.Instance.ListPorts();

        if (ports == null || ports.Count == 0)
        {

            Console.WriteLine("No se encontraron puertos disponibles.");

            return;

        }

        Console.WriteLine("Puertos disponibles:");

        for (int i = 0; i < ports.Count; i++)
        {

            Console.WriteLine($"{i + 1}: {ports[i]}");

        }

        Console.WriteLine("Seleccione el número del puerto a utilizar:");

        int portIndex;

        string portInput = Console.ReadLine();

        while (!int.TryParse(portInput, out portIndex) || portIndex < 1 || portIndex > ports.Count)
        {

            Console.WriteLine("Selección inválida. Intente nuevamente:");

            portInput = Console.ReadLine();

        }

        string selectedPort = ports[portIndex - 1];

        try
        {

            POSAutoservicio.Instance.OpenPort(selectedPort);

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

            Task<SaleResponse> response = POSAutoservicio.Instance.Sale(amount, ticket, true, true);

            response.Wait();

            // Crear instancia del printer
            EpsonTM30Printer printer = new EpsonTM30Printer("EPSON TM-m30 Receipt");

            // Verificar que la impresora esté disponible
            if (!printer.IsPrinterAvailable())
            {

                Console.WriteLine("Impresora no encontrada. Impresoras disponibles:");
                
                printer.ListAvailablePrinters();
                
                return;
            
            }

            // Imprimir el resultado
            await printer.PrintSaleResult(response.Result.Response);

        }

        catch (Exception ex)
        {

            Console.WriteLine("Error al realizar la venta: " + ex.Message);

        }

        finally
        {

            POSAutoservicio.Instance.ClosePort();

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

public class EpsonTM30Printer
{

    private string _printerName;

    public EpsonTM30Printer(string printerName = null)
    {

        _printerName = printerName ?? "EPSON TM-m30 Receipt"; // Nombre por defecto

    }

    // Método principal para imprimir el resultado de la venta
    public async Task PrintSaleResult(string saleResponse)
    {

        try
        {

            // Formatear el comprobante
            string receiptText = FormatTransbankReceipt(saleResponse);

            // Imprimir usando PrintDocument
            PrintReceipt(receiptText);

            Console.WriteLine("Comprobante impreso exitosamente");

        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error al imprimir: {ex.Message}");

        }

    }

    // Método para imprimir usando PrintDocument
    public void PrintReceipt(string receiptText)
    {

        try
        {

            PrintDocument printDocument = new PrintDocument();

            printDocument.PrinterSettings.PrinterName = _printerName;

            // Configurar el papel para impresora térmica (80mm)
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 600); // 80mm de ancho

            printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            printDocument.PrintPage += (sender, e) =>
            {

                PrintReceiptContent(e, receiptText);

            };

            printDocument.Print();

            printDocument.Dispose();

        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error al imprimir: {ex.Message}");

        }

    }

    // Método para imprimir el contenido del comprobante
    private void PrintReceiptContent(PrintPageEventArgs e, string receiptText)
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