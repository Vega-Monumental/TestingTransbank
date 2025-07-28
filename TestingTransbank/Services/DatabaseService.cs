using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalidaAutomaticaQR.Models;

namespace TestingTransbank.Services
{
    public class DatabaseService
    {

        public async Task<bool> TestConnectionAsync(string connectionString)
        {

            try
            {

                using (var context = new EstacionamientoContext(connectionString))
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

    }

}
