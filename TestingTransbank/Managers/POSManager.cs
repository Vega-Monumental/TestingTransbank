using SalidaAutomaticaQR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingTransbank.Helpers;
using TestingTransbank.Services;
using Transbank.POSAutoservicio;
using Transbank.Responses.CommonResponses;

namespace TestingTransbank.Managers
{
    public class POSManager
    {

        private readonly POSService _posService;

        private readonly PrinterService _printerService;

        private readonly TicketService _ticketService;

        public POSManager(POSService posService, PrinterService printerService, TicketService ticketService)
        {

            _posService = posService;

            _printerService = printerService;

            _ticketService = ticketService;
        
        }

        public async Task SelectOperationLoop()
        {

            try
            {

                bool @continue = true;

                while (@continue)
                {

                    POSAutoservicio.Instance.OpenPort(ConfigurationHelper.SelectedPort);

                    Console.WriteLine("\nSeleccione la acción a realizar:");

                    Console.WriteLine("1. Verificar conexión");

                    Console.WriteLine("2. Cargar llaves");

                    Console.WriteLine("3. Inicializar");

                    Console.WriteLine("4. Venta básica");

                    Console.WriteLine("5. Venta completa (Operacíón Vega Monumental)");

                    Console.WriteLine("6. Cierre");

                    Console.WriteLine("7. Salir");

                    Console.Write("Ingrese el número de la opción: ");

                    int optionIndex;

                    string optionInput = Console.ReadLine();

                    if (int.TryParse(optionInput, out optionIndex) && optionIndex >= 1 && optionIndex <= 7)
                    {

                        switch (optionIndex)
                        {

                            case 1:

                                await PollPOS();

                                break;

                            case 2:

                                await LoadKeysPOS();

                                break;

                            case 3:

                                await InitializePOS();

                                break;

                            case 4:

                                await BasicSalePOS();

                                break;


                            case 5:

                                //await PersonalizedSalePOS();

                                break;


                            case 6: // Cierre

                                await ClosePOS();

                                break;


                            case 7: // Salir
                                {

                                    @continue = false;

                                    break;

                                }

                        }

                    }

                    else
                    {

                        Console.WriteLine("\nOpción inválida. Intente nuevamente.");

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

        public async Task PollPOS ()
        {

            Task<bool> pollResult = POSAutoservicio.Instance.Poll();

            pollResult.Wait();

            if (pollResult.Result)
            {

                Console.WriteLine("Pos Connected");

            }

            else
            {

                Console.WriteLine("Pos NOT Connected");

            }

        }

        public async Task LoadKeysPOS()
        {

            Task<LoadKeysResponse> loadKeyResponse = POSAutoservicio.Instance.LoadKeys();

            loadKeyResponse.Wait();

            Console.WriteLine(loadKeyResponse.Result);

            if (loadKeyResponse.Result.Success)
            {

                Console.WriteLine("Carga de llaves exitosa.");

            }

            else
            {

                Console.WriteLine("No se pudo realizar la carga de llaves.");

            }

        }

        public async Task InitializePOS()
        {

            Task<bool> initializationResult = POSAutoservicio.Instance.Initialization();

            initializationResult.Wait();

            if (initializationResult.Result)
            {

                Console.WriteLine("Pos Initialized");

            }

            else
            {

                Console.WriteLine("Pos NOT Initialized");

            }

        }

        public async Task BasicSalePOS()
        {

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

            POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

            var saleResponse = POSAutoservicio.Instance.Sale(amount, ticket, true, false);

            saleResponse.Wait();

            POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

            Console.WriteLine("Venta realizada exitosamente:");

            _printerService.PrintTransbankVoucher(saleResponse.Result.Response, ReceiptType.Sale);

        }

        public async Task PersonalizedSalePOS()
        {

            StringBuilder buffer = new StringBuilder();

            while (true)
            {

                Console.WriteLine($"\nEsperando lectura de código de barras...");

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {

                    string barcode = buffer.ToString();

                    buffer.Clear();

                    if (string.IsNullOrWhiteSpace(barcode))
                    {

                        Console.WriteLine($"\nCódigo el ticket no es válido.");

                        continue;

                    }

                    Ticket? ticket = await _ticketService.GetTicketByIdAsync(barcode);

                    if (ticket == null)
                    {

                        Console.WriteLine($"\nTicket no encontrado. Intente nuevamente.");

                        continue;

                    }

                    bool isValidTicket = _ticketService.ValidateTicketForPayment(ticket);

                    if (!isValidTicket)
                    {

                        Console.WriteLine("\nTicket no válido.");

                        continue;


                    }

                    int amount = await _ticketService.GetTicketAmountAsync(ticket);

                    if (amount == 0)
                    {

                        Console.WriteLine("\nEl ticket no tiene monto a pagar.");

                        continue;

                    }

                    if (amount > 0)
                    {

                        ticket.Monto = amount;

                        POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                        var saleResponse = POSAutoservicio.Instance.Sale(amount, ticket.NumTicket.ToString(), true, false);

                        saleResponse.Wait();

                        if (saleResponse.Result.Success)
                        {

                            Console.WriteLine($"Venta realizada exitosamente: {saleResponse.Result.Response}");

                            POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                            _printerService.PrintTransbankVoucher(saleResponse.Result.Response, ReceiptType.Sale);

                            int sheet = await _ticketService.GetSheetAsync(ticket);

                            ticket.NumBoleta = sheet;

                            #region Imprimir boleta

                            await _printerService.PrintBussinesVoucher(ticket);

                            #endregion

                            break;

                        }

                        if (!saleResponse.Result.Success)
                        {

                            Console.WriteLine("Venta realizada exitosamente:");

                        }

                    }

                }

                else
                {

                    // Acumula cada carácter leído
                    buffer.Append(keyInfo.KeyChar);

                }

            }

        }

        public async Task ClosePOS ()
        {

            POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

            var closeResponse = POSAutoservicio.Instance.Close(true);

            closeResponse.Wait();

            POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

            Console.WriteLine("Cierre realizado:");

            _printerService.PrintTransbankVoucher(closeResponse.Result.Response, ReceiptType.Close);

        }

    }

}
