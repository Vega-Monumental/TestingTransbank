using BarcodeStandard;
using SalidaAutomaticaQR.Models;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using TestingTransbank.Helpers;
using TestingTransbank.Repositories;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace TestingTransbank.Services
{
    public class PrinterService
    {

        private readonly PrinterRepository _printerRepository;

        private readonly CAFRepository _cafRepository;

        public PrinterService(CAFRepository cafRepository, PrinterRepository printerRepository)
        {

            _cafRepository = cafRepository;

            _printerRepository = printerRepository;

        }

        public List<string> GetPrinters()
        {

            return _printerRepository.GetPrinters();

        }

        #region Transbank voucher
        public void PrintTransbankVoucher(string voucher, ReceiptType receiptType)
        {

            try
            {

                string receiptText;

                switch (receiptType)
                {

                    case ReceiptType.Sale:


                        receiptText = FormatTransbankSale(voucher);

                        Console.WriteLine("Imprimiendo voucher de venta...");

                        break;

                    case ReceiptType.Close:

                        receiptText = FormatTransbankClose(voucher);

                        Console.WriteLine("Imprimiendo voucher de cierre...");

                        break;

                    default:

                        throw new ArgumentException("Tipo de recibo no soportado");

                }

                PrintDocument printDocument = new PrintDocument();

                printDocument.PrinterSettings.PrinterName = ConfigurationHelper.SelectedPrinter;

                printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 600);

                printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                printDocument.PrintPage += (sender, e) =>
                {

                    if (receiptType == ReceiptType.Sale)

                        LayoutTransbankSaleContent(e, receiptText);

                    if (receiptType == ReceiptType.Close)

                        LayoutTransbankCloseContent(e, receiptText);

                };

                printDocument.Print();

                printDocument.Dispose();

                Console.WriteLine("Voucher impreso exitosamente");

            }

            catch (Exception ex)
            {

                Console.WriteLine($"Error al imprimir: {ex.Message}");

            }

        }

        // Método para imprimir el contenido del comprobante
        private void LayoutTransbankSaleContent(PrintPageEventArgs e, string receiptText)
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
        private void LayoutTransbankCloseContent(PrintPageEventArgs e, string closeText)
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
        public string FormatTransbankSale(string rawResponse)
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

        #endregion

        #region Bussines voucher

        public async Task PrintBussinesVoucher(Ticket ticket)
        {

            try
            {

                Barcode barcode = new Barcode();

                barcode.IncludeLabel = true;

                // Convertir System.Drawing.Color a SKColorF
                var black = new SKColor(0, 0, 0); // Negro

                var white = new SKColor(255, 255, 255); // Blanco

                SKImage skImage = barcode.Encode(BarcodeStandard.Type.Code128, ticket.NumTicket.ToString(), black, white, 110, 110);

                Image printableBarcode = ConvertSKImageToSystemImage(skImage);

                string TED = await GenerateTED(ticket);

                Bitmap PDF417 = GeneratePDF417(TED);

                PrintDocument printDocument = new PrintDocument();

                printDocument.PrinterSettings.PrinterName = ConfigurationHelper.SelectedPrinter;

                printDocument.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", (int)(80 * 3.937), 500); //315, 500);

                printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                printDocument.PrintPage += (sender, e) => {

                    LayoutBussinessSaleContent(e, ticket, printableBarcode, PDF417);

                };

                printDocument.Print();

                printDocument.Dispose();

                Console.WriteLine("Boleta impresa exitosamente");

            }

            catch (Exception ex)
            {

                Console.WriteLine($"Error al imprimir: {ex.Message}");

            }

        }

        static Image ConvertSKImageToSystemImage(SKImage skImage)
        {

            // Convertir SKImage a datos PNG
            SKData skData = skImage.Encode(SKEncodedImageFormat.Png, 100);

            // Crear un MemoryStream con los datos
            using (MemoryStream stream = new MemoryStream(skData.ToArray()))
            {

                // Crear System.Drawing.Image desde el stream
                return Image.FromStream(stream);
            }

        }

        public async Task<string> GenerateTED(Ticket ticket)
        {

            var caf = await _cafRepository.GetCAFAsync();

            XDocument caf_to_xml = XDocument.Parse(caf.XmlArchivo);

            var rutEmisor = caf_to_xml.Descendants().Where(n => n.Name == "RE").FirstOrDefault().Value;

            var razonSocial = caf_to_xml.Descendants().Where(n => n.Name == "RS").FirstOrDefault().Value;

            var tipoDocumento = caf_to_xml.Descendants().Where(n => n.Name == "TD").FirstOrDefault().Value;

            var desde = caf_to_xml.Descendants().Where(n => n.Name == "D").FirstOrDefault().Value;

            var hasta = caf_to_xml.Descendants().Where(n => n.Name == "H").FirstOrDefault().Value;

            var fecha = caf_to_xml.Descendants().Where(n => n.Name == "FA").FirstOrDefault().Value;

            var rsapk = caf_to_xml.Descendants().Where(n => n.Name == "M").FirstOrDefault().Value;

            var firma = caf_to_xml.Descendants().Where(n => n.Name == "FRMA").FirstOrDefault().Value;

            XDocument ted = new XDocument(

                new XElement("TED", new XAttribute("version", "1.0"),

                    new XElement("DD",

                        new XElement("RE", rutEmisor), //rut emisor

                        new XElement("TD", tipoDocumento),//tipo documento

                        new XElement("F", ticket.NumBoleta), //folio

                        new XElement("FE", DateTime.Now.ToShortDateString()),

                        new XElement("RR", "66666666-6"),

                        new XElement("RSR", "DESCONOCIDO"),

                        new XElement("MNT", ticket.Monto), //monto emitido

                        new XElement("IT1", "ESTACIONAMIENTO"), //item 

                        new XElement("CAF", new XAttribute("version", "1.0"),

                        new XElement("DA",

                            new XElement("RE", rutEmisor), //rut emisor

                            new XElement("RS", razonSocial), //razón social

                            new XElement("TD", tipoDocumento), //tipo documento

                            new XElement("RNG", //rango folios

                                new XElement("D", desde), //desde

                                new XElement("H", hasta)

                            ), //hasta

                            new XElement("FA", fecha), //fecha autorización

                            new XElement("RSAPK", //firma

                                new XElement("M", rsapk),

                                new XElement("E", "Aw==")

                            ),

                            new XElement("IDK", 300)

                        ),

                        new XElement("FRMA", new XAttribute("algoritmo", "SHA1withRSA"), firma)

                    ),

                    new XElement("TSTED", DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + "T" + DateTime.Now.ToShortTimeString())

                    ), //</DD>

                    new XElement("FRMT", new XAttribute("algoritmo", "SHA1withRSA"), "GuLwa4WbGqcYeP+JDpDEgVLnHpDNP5j94d4D4ogffwk7Px+dIM+N5pc6vMQ6pvHVhBBPnQo/Tyo86E7PBHZtHA==")

                )

            );

            ted.Declaration = new XDeclaration("1.0", "ISO-8859-1", null);

            string[] xmlWithouthSpaces = ted.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i <= xmlWithouthSpaces.Length - 1; i++)

                xmlWithouthSpaces[i] = xmlWithouthSpaces[i].TrimStart();

            string formattedXML = string.Join("\r\n", xmlWithouthSpaces);

            formattedXML = formattedXML.Replace("\r", "").Replace("\n", "");

            ted = XDocument.Parse(formattedXML, LoadOptions.PreserveWhitespace);

            return formattedXML;

        }

        public Bitmap GeneratePDF417(string TED)
        {

            // Codificar el texto en ISO-8859-1
            Encoding ISO = Encoding.GetEncoding("ISO-8859-1");

            byte[] ISOBytes = ISO.GetBytes(TED);

            string codificatedTED = ISO.GetString(ISOBytes); // ZXing trabaja con string, no con byte[]

            // Configurar el generador de PDF417
            var writer = new BarcodeWriter<Bitmap>
            {

                Format = BarcodeFormat.PDF_417,

                Renderer = new BitmapRenderer(),

                Options = new EncodingOptions
                {

                    Width = 300,  // Ancho fijo para mejor calidad

                    Height = 100,  // Alto fijo para mejor calidad

                    Margin = 2,   // Margen para evitar cortes

                    PureBarcode = false // Incluir zona silenciosa

                }

            };

            Bitmap bitmap = writer.Write(codificatedTED);

            Bitmap enhancedBitmap = EnhanceBitmapQuality(bitmap);

            return enhancedBitmap;

        }

        // Método para mejorar la calidad del bitmap
        private Bitmap EnhanceBitmapQuality(Bitmap originalBitmap)
        {
            // Crear una nueva imagen con mejor resolución
            Bitmap enhancedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format24bppRgb);
            enhancedBitmap.SetResolution(300, 300); // Aumentar DPI para mejor calidad de impresión

            using (Graphics g = Graphics.FromImage(enhancedBitmap))
            {
                // Configurar para mejor calidad
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.None; // Importante para códigos de barras

                // Dibujar la imagen original en la nueva
                g.DrawImage(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height);
            }

            return enhancedBitmap;
        }

        private void LayoutBussinessSaleContent(PrintPageEventArgs e, Ticket ticket, Image barcode, Bitmap PDF417)
        {
            Graphics g = e.Graphics;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;


            Font cabecera = new Font("CALIBRI", (float)9, FontStyle.Regular);
            Font fBody = new Font("CALIBRI", (float)9, FontStyle.Regular);
            Font pie = new Font("CALIBRI", (float)9, FontStyle.Regular);
            SolidBrush sb = new SolidBrush(Color.Black);
            int y = 2;
            int SPACE = 20;
            RectangleF rect = new RectangleF(5, SPACE + 290, 250, 300);
            //RectangleF pdf = new RectangleF(5, SPACE + 310, 250, 100);

            RectangleF desc = new RectangleF(5, SPACE + 410, 250, 100);
            //CABECERA
            g.DrawString("COMERCIALIZADORA VEGA MONUMENTAL S.A.", cabecera, sb, y, SPACE);
            g.DrawString("CM: AVENIDA 21 DE MAYO #3215 P2 - CONCEPCIÓN", cabecera, sb, y, SPACE + 15);
            g.DrawString("SUC 1: AVDA. 21 DE MAYO #3225 - CONCEPCIÓN", cabecera, sb, y, SPACE + 30);
            g.DrawString("SUC 2: CAPITÁN ORELLA #175 - CONCEPCIÓN", cabecera, sb, y, SPACE + 45);
            g.DrawString("RUT Nro.: 76.126.876-7", cabecera, sb, y, SPACE + 60);
            g.DrawString("COMERCIALIZADORA DE LOCALES Y SERVICIOS", cabecera, sb, y, SPACE + 75);
            g.DrawString($"FECHA: {DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}", cabecera, sb, y, SPACE + 90);
            g.DrawString($"HORA SALIDA: {DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}", cabecera, sb, 280, SPACE + 90, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString($"HORA ENTRADA: {ticket.Horaentrada} ", cabecera, sb, 280, SPACE + 105, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            //CUERPO
            g.DrawString("Nro. Boleta electrónica", fBody, sb, y, SPACE + 120);
            g.DrawString($"{ticket.NumBoleta}", fBody, sb, 280, SPACE + 120, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Tarifa", fBody, sb, y, SPACE + 135);

            //g.DrawString("", fBody, sb, y, SPACE + 115);
            g.DrawString($"ESTACIONAMIENTO", fBody, sb, y, SPACE + 165);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 165, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Monto neto", fBody, sb, y, SPACE + 180);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 180, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("I.V.A. (19%)", fBody, sb, y, SPACE + 195);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 195, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Total", fBody, sb, y, SPACE + 210);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 210, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Efectivo", fBody, sb, y, SPACE + 225);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 225, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Suma de sus pagos", fBody, sb, y, SPACE + 240);
            g.DrawString($"{ticket.Monto}", fBody, sb, 280, SPACE + 240, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            g.DrawString("Su vuelto", fBody, sb, y, SPACE + 255);
            g.DrawString($"{0}", fBody, sb, 280, SPACE + 255, new StringFormat(StringFormatFlags.DirectionRightToLeft));
            //PIE DE HOJA
            g.DrawString("Gracias por su preferencia", pie, sb, rect, sf);
            //PDF417
            RectangleF cb = new RectangleF(5, SPACE + 310, 250, 80);
            g.DrawImage(barcode, cb);

            g.DrawString("Res. 74 del 2012. Timbre electrónico SII", pie, sb, desc, sf);
            g.DrawString("Verifique su documento en: http://www.sii.cl", pie, sb, y, SPACE + 425);
            g.DrawString($"{ticket.NumBoleta}", fBody, sb, 280, SPACE + 120, new StringFormat(StringFormatFlags.DirectionRightToLeft));


            RectangleF pdf = new RectangleF(0, SPACE + 450, 270, 90); // PDF417 arriba
            g.DrawImage(PDF417, pdf);
            // Indicate that no more data to print, and the Print Document can now send the print data to the spooler.
            e.HasMorePages = false;
        }

        #endregion

    }
}
