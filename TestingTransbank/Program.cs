using Transbank.POSAutoservicio;
using Transbank.Responses.CommonResponses;
using Transbank.Responses.AutoservicioResponse;

class Program
{
    static void Main(string[] args)
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

            Console.WriteLine(response);

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