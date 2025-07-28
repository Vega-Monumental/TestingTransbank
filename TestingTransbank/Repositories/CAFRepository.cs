using Microsoft.EntityFrameworkCore;
using SalidaAutomaticaQR.Models;

namespace TestingTransbank.Services
{
    public class CAFRepository
    {

        public async Task<CAF?> GetCAFAsync()
        {

            try
            {

                using (var context = new EstacionamientoContext())
                {

                    var CAF = await context.CAF.FirstOrDefaultAsync();

                    return CAF;

                }

            }

            catch (Exception ex)
            {

                Console.WriteLine($"\nError al consultar la base de datos: {ex.Message}");

                return null;

            }

        }

    }

}
