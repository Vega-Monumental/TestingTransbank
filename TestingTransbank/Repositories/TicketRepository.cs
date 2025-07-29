using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalidaAutomaticaQR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingTransbank.Repositories
{
    public class TicketRepository
    {

        public async Task<Ticket?> GetTicketByIdAsync(string id)
        {

            try
            {

                using (var context = new EstacionamientoContext())
                {

                    int idToInt;

                    if (!Int32.TryParse(id, out idToInt))
                    {

                        Console.WriteLine($"\nEl código ingresado no es válido.");

                        return null;

                    }

                    var boleta = await context.Ticket

                        .Where(b => b.NumTicket == idToInt)

                        .FirstOrDefaultAsync();

                    return boleta;

                }

            }

            catch (Exception ex)
            {

                Console.WriteLine($"\nError al consultar la base de datos: {ex.Message}");

                return null;

            }

        }

        public async Task<int> GetSheetAsync(Ticket ticket)
        {

            using (var context = new EstacionamientoContext())
            {

                var numTicket = new SqlParameter("@num_ticket", ticket.NumTicket);

                var monto = new SqlParameter("@monto", ticket.Monto);

                var fechasalida = new SqlParameter("@fechasalida", DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));

                var horasalida = new SqlParameter("@horasalida", DateTime.Now.TimeOfDay);

                var horaentrada = new SqlParameter("@horaentrada", ticket.Horaentrada);

                var codTurno = new SqlParameter("@cod_turno", ticket.CodTurno);

                var codUsuario = new SqlParameter("@cod_usuario", ticket.CodUsuario);

                var codTipoUsuario = new SqlParameter("@cod_tipo_usuario", ticket.CodTipoUsuario);

                var estado = new SqlParameter("@estado", "0");

                var numCaja = new SqlParameter("@num_caja", ticket.NumCaja);

                // Parámetro de salida
                var folioOutput = new SqlParameter("@folio", SqlDbType.Int)
                {

                    Direction = ParameterDirection.Output
                };


                await context.Database.ExecuteSqlRawAsync(

                    "EXEC dbo.sp_ObtenerFolio @num_ticket, @monto, @fechasalida, @horasalida, @horaentrada, @cod_turno, @cod_usuario, @cod_tipo_usuario, @estado, @num_caja, @folio OUTPUT",

                    numTicket, monto, fechasalida, horasalida, horaentrada, codTurno, codUsuario, codTipoUsuario, estado, numCaja, folioOutput

                );


                var returnFolio = (int)folioOutput.Value;

                ticket.NumBoleta = returnFolio;

                return returnFolio;

            }

        }

    }

}
