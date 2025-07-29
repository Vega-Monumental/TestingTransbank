using Microsoft.EntityFrameworkCore;
using SalidaAutomaticaQR.Models;

namespace TestingTransbank.Repositories
{
    public class VariableRepository
    {

        public async Task<Variable?> GetVariablesAsync()
        {

            try
            {

                using (var context = new EstacionamientoContext())
                {

                    var variables = await context.Variables.FirstOrDefaultAsync();

                    return variables;

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
