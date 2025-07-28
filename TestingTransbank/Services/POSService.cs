using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transbank.Responses.CommonResponses;

namespace TestingTransbank.Services
{

    public class POSService
    {

        public void NewIntermediateMessageReceived(object sender, IntermediateResponse e)
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

}
