using Azure;
using NAudio.CoreAudioApi;
using SalidaAutomaticaQR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestingTransbank.Helpers;
using TestingTransbank.Models;
using TestingTransbank.Services;
using Transbank.POSAutoservicio;
using Transbank.Responses.AutoservicioResponse;
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

                    Console.WriteLine("Seleccione la acción a realizar:");

                    Console.WriteLine();

                    Console.WriteLine("1. Verificar conexión");

                    Console.WriteLine("2. Cargar llaves");

                    Console.WriteLine("3. Inicializar");

                    Console.WriteLine("4. Respuesta de inicialización");

                    Console.WriteLine("5. Venta básica");

                    Console.WriteLine("6. Venta completa (Operacíón Vega Monumental)");

                    Console.WriteLine("7. Venta Multicode");

                    Console.WriteLine("8. Última venta");

                    Console.WriteLine("9. Reembolso");

                    Console.WriteLine("10. Cierre");

                    Console.WriteLine("11. Salir");

                    Console.WriteLine();

                    Console.WriteLine("Ingrese el número de la opción: ");

                    Console.WriteLine();

                    int optionIndex;

                    string optionInput = Console.ReadLine();

                    Console.WriteLine();

                    if (int.TryParse(optionInput, out optionIndex) && optionIndex >= 1 && optionIndex <= 11)
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

                                await InitializeResponsePOS();

                                break;


                            case 5:

                                await BasicSalePOS();

                                break;


                            case 6:

                                await PersonalizedSalePOS();

                                break;


                            case 7:

                                await MulticodeSalePOS();

                                break;


                            case 8:

                                await LastSalePOS();

                                break;

                            case 9:

                                await RefundPOS();

                                break;


                            case 10:

                                await ClosePOS();

                                break;

                            case 11:

                                @continue = false;

                                break;

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

                Console.WriteLine("\nError al realizar la venta: " + ex.Message);

            }

        }

        public async Task PollPOS ()
        {


            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Task<bool> response = POSAutoservicio.Instance.Poll();

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result)
                {

                    Console.WriteLine("POS conectado");

                    Console.WriteLine();


                }

                else
                {

                    Console.WriteLine("POS no conectado");

                    Console.WriteLine();

                }


            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }


        }

        public async Task LoadKeysPOS()
        {

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Task<LoadKeysResponse> response = POSAutoservicio.Instance.LoadKeys();

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {


                    Console.WriteLine("Carga de llaves exitosa.");

                    Console.WriteLine();

                }

                else
                {

                    Console.WriteLine("No se pudo realizar la carga de llaves.");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }


        }

        public async Task InitializePOS()
        {

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Task<bool> response = POSAutoservicio.Instance.Initialization();

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result)
                {

                    Console.WriteLine("POS inicializado exitosamente, verifique el estado presionando (4)");

                    Console.WriteLine();

                }

                else
                {

                    Console.WriteLine("POS no inicializado");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }

        }

        public async Task InitializeResponsePOS()
        {

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Task<InitializationResponse> response = POSAutoservicio.Instance.InitializationResponse();

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {

                    Console.WriteLine("POS verificado exitosamente");

                    Console.WriteLine();

                }

                else
                {

                    Console.WriteLine("No se logró verificar la respuesta de inicialización");

                    Console.WriteLine();

                }
            }
            
            catch (Exception e)
            {
            
                Console.WriteLine("Error: " + e.Message);
                
            
            }

        }

        public async Task BasicSalePOS()
        {

            Console.WriteLine("Ingrese el monto a solicitar:");

            Console.WriteLine();

            string input = Console.ReadLine();

            Console.WriteLine();

            int amount;

            while (!int.TryParse(input, out amount) || amount < 50)
            {

                Console.WriteLine("Monto inválido. Debe ser un número mayor o igual a 50. Intente nuevamente:");

                input = Console.ReadLine();

            }

            Console.WriteLine("Ingrese el ticket (máx 20 caracteres):");

            Console.WriteLine();

            string ticket = Console.ReadLine();

            Console.WriteLine();

            if (ticket.Length > 20)
            {

                ticket = ticket.Substring(0, 20);

                Console.WriteLine("El ticket fue truncado a 20 caracteres.");

                Console.WriteLine();

            }

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Task<SaleResponse> response = POSAutoservicio.Instance.Sale(amount, ticket, true, true);

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {


                    Console.WriteLine("Venta realizada con éxito");

                    Console.WriteLine();

                    _printerService.PrintTransbankVoucher(response.Result.Response, ReceiptType.Sale);

                }

                else
                {

                    Console.WriteLine("No se logró realizar la venta");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);


            }


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

        public async Task MulticodeSalePOS()
        {


            Console.WriteLine("Ingrese el monto a solicitar:");

            Console.WriteLine();

            string input = Console.ReadLine();

            Console.WriteLine();

            int amount;

            while (!int.TryParse(input, out amount) || amount < 50)
            {

                Console.WriteLine("Monto inválido. Debe ser un número mayor o igual a 50. Intente nuevamente:");

                input = Console.ReadLine();

            }

            Console.WriteLine("Ingrese el ticket (máx 20 caracteres):");

            Console.WriteLine();

            string ticket = Console.ReadLine();

            Console.WriteLine();

            if (ticket.Length > 20)
            {

                ticket = ticket.Substring(0, 20);

                Console.WriteLine("El ticket fue truncado a 20 caracteres.");

                Console.WriteLine();

            }

            // Lista con los valores
            List<long> commerceCodes = new List<long>
            {

                597029414300,

                597029414301,

                597029414302,

                597029414303,

                597029414304,

                597029414305,

                597029414306,

                597029414307,

                597029414308

            };

            Console.WriteLine("Códigos de comercio disponibles:");

            Console.WriteLine();

            for (int i = 0; i < commerceCodes.Count; i++)
            {

                Console.WriteLine($"{i + 1}: {commerceCodes[i]}");

            }

            Console.WriteLine($"\nSeleccione el numero del código de comercio a utilizar (1 - {commerceCodes.Count}):");

            int commerceCodeIndex;

            Console.WriteLine();

            string commerceCodeInput = Console.ReadLine();

            Console.WriteLine();

            while (!int.TryParse(commerceCodeInput, out commerceCodeIndex) || commerceCodeIndex < 1 || commerceCodeIndex > commerceCodes.Count)
            {

                Console.WriteLine("Selección inválida. Intente nuevamente:");

                commerceCodeInput = Console.ReadLine();

            }

            long selectedCommerceCode = commerceCodes[commerceCodeIndex - 1];

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Task<MultiCodeSaleResponse> response = POSAutoservicio.Instance.MultiCodeSale(amount, ticket, selectedCommerceCode, true, false);

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {

                    Console.WriteLine("Venta realizada con éxito");

                    Console.WriteLine();

                    _printerService.PrintTransbankVoucher(response.Result.Response, ReceiptType.Sale);

                }

                else
                {

                    Console.WriteLine("No se logró realizar la venta");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }

        }

        public async Task LastSalePOS()
        {

            try
            {


                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Task<LastSaleResponse> response = POSAutoservicio.Instance.LastSale(true);

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {

                    Console.WriteLine("Última venta obtenida con éxito");

                    Console.WriteLine();

                    _printerService.PrintTransbankVoucher(response.Result.Response, ReceiptType.Sale);

                }

                else
                {

                    Console.WriteLine("No se pudo obtener la última venta");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);


            }

        }

        public async Task RefundPOS()
        {

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Task<RefundResponse> response = POSAutoservicio.Instance.Refund();

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {

                    Console.WriteLine("Reembolso realizado con éxito");

                    Console.WriteLine();

                }

                else
                {

                    Console.WriteLine("No se pudo realizar el reembolso");

                    Console.WriteLine();

                }

            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }

        }

        public async Task ClosePOS()
        {

            try
            {

                POSAutoservicio.Instance.IntermediateResponseChange += _posService.NewIntermediateMessageReceived;

                Task<CloseResponse> response = POSAutoservicio.Instance.Close(true);

                response.Wait();

                POSAutoservicio.Instance.IntermediateResponseChange -= _posService.NewIntermediateMessageReceived;

                Console.WriteLine();

                Console.WriteLine(response.Result);

                Console.WriteLine();

                if (response.Result.Success)
                {

                    Console.WriteLine("Cierre realizado con éxito");

                    Console.WriteLine();

                    _printerService.PrintTransbankVoucher(response.Result.Response, ReceiptType.Close);

                }

                else
                {

                    Console.WriteLine("No se pudo realizar el cierre");

                    Console.WriteLine();

                }


            }

            catch (Exception e)
            {

                Console.WriteLine("Error: " + e.Message);

            }

        }

    }

}
