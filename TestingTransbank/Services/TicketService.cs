using SalidaAutomaticaQR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingTransbank.Helper;

namespace TestingTransbank.Services
{
    public class TicketService
    {

        private readonly TicketRepository _ticketRepository;

        private readonly VariableRepository _variableRepository;

        public TicketService(TicketRepository ticketRepository, VariableRepository variableRepository)
        {

            _ticketRepository = ticketRepository;

            _variableRepository = variableRepository;

        }

        public async Task<Ticket?> GetTicketByIdAsync(string id)
        {

            return await _ticketRepository.GetTicketByIdAsync(id);

        }

        public bool ValidateTicketForPayment(Ticket ticket)
        {

            if (ticket.Estado == 2)
            {

                AudioHelper.PlayAudio(AudioPaths.TicketNoValido);

                Console.WriteLine($"\n El ticket ya se utilizó para salir.");

                return false;

            }

            if (ticket.Estado == 1)
            {

                AudioHelper.PlayAudio(AudioPaths.TicketAprobado);

                Console.WriteLine($"\nEl ticket se encuentra disponible para pagar.");

                return true;

            }

            if (ticket.Estado == 0)
            {

                Console.WriteLine($"\nEl ticket se encuentra pagado.");

                return false;

            }

            return false;
        }

        public async Task<int> GetTicketAmountAsync(Ticket ticket)
        {

            var variables = await _variableRepository.GetVariablesAsync();

            if (variables == null)
            {

                Console.WriteLine("\nNo se pudieron cargar las variables del sistema.");

                return 0;

            }

            // Combinar fecha y hora de entrada en un DateTime
            DateTime fechaHoraEntrada = ticket.Fechaentrada.ToDateTime(ticket.Horaentrada);

            // Obtener la fecha y hora actual
            DateTime fechaHoraSalida = DateTime.Now;

            // Calcular la diferencia
            TimeSpan tiempoTranscurrido = fechaHoraSalida - fechaHoraEntrada;

            double minutosFraccional = tiempoTranscurrido.TotalMinutes;

            int minutosRedondeados = (int)Math.Round(minutosFraccional);

            if (minutosRedondeados <= variables.MinutosLibres)
            {

                return 0; // Tiempo gratis

            }

            else
            {

                int amount = (int)(minutosRedondeados * variables.ValorMinuto);

                return amount;

            }

        }

        public async Task<int> GetSheetAsync(Ticket ticket)
        {

            return await _ticketRepository.GetSheetAsync(ticket);

        }

    }

}

