using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeStandard;
using SalidaAutomaticaQR.Models;
using SkiaSharp;
using System.Xml.Linq;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using ZXing;
using Transbank.POSAutoservicio;
using TestingTransbank.Services;
using TestingTransbank.Helpers;

namespace TestingTransbank.Managers
{
    public class PrinterManager
    {

        private PrinterService _printerService;

        public PrinterManager(PrinterService printerService)
        {

            _printerService = printerService;


        }

        public void SelectPrinter()
        {

            List<string> printers = _printerService.GetPrinters();

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

            string printer = printers[printerIndex - 1];

            ConfigurationHelper.SetSelectedPrinter(printer);

        }

        public void SelectPort()
        {

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

            ConfigurationHelper.SetSelectedPort(ports[portIndex - 1]);

        }

    }

}
