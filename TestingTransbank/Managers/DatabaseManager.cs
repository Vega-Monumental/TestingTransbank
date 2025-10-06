using TestingTransbank.Helpers;
using TestingTransbank.Models;
using TestingTransbank.Services;

namespace TestingTransbank.Managers
{
    public class DatabaseManager
    {

        private DatabaseService _databaseService;

        public DatabaseManager(DatabaseService databaseService)
        {

            _databaseService = databaseService;

        }

        public async Task SelectDatabase()
        {

            List<DatabaseInfo> databasesInfo = ConfigurationHelper.AvailableDatabases;

            bool conexionExitosa = false;

            Console.WriteLine("Bases de datos disponibles:");

            Console.WriteLine();

            for (int i = 0; i < databasesInfo.Count; i++)
            {

                Console.WriteLine($"{i + 1}. {databasesInfo[i].Database}");

            }

            Console.WriteLine($"\nSeleccione el número de la base de datos a utilizar (1 - {databasesInfo.Count}):");

            while (!conexionExitosa)
            {

                int databaseIndex;

                Console.WriteLine();

                string databaseInput = Console.ReadLine();

                Console.WriteLine();

                while (!int.TryParse(databaseInput, out databaseIndex) || databaseIndex < 1 || databaseIndex > databasesInfo.Count)
                {

                    Console.WriteLine($"\nOpción inválida. Debe ser un número entre 1 y {databasesInfo.Count}");

                }

                string selectedDatabase = databasesInfo[databaseIndex - 1].Database;

                string selectedConnectionString = databasesInfo[databaseIndex - 1].ConnectionString;

                Console.WriteLine($"Probando conexión a: {ConfigurationHelper.SelectedDatabase}...");

                // Probar la conexión
                conexionExitosa = await _databaseService.TestConnectionAsync(selectedConnectionString);

                if (conexionExitosa)
                {

                    Console.WriteLine($"Conexión establecida: {ConfigurationHelper.SelectedDatabase}");

                    ConfigurationHelper.SetSelectedDatabase(databasesInfo[databaseIndex - 1].Database);

                    ConfigurationHelper.SetSelectedConnectionString(databasesInfo[databaseIndex - 1].ConnectionString);

                    break;

                }

                else
                {

                    Console.WriteLine($"Error: No se pudo establecer conexión con {selectedDatabase}");

                    Console.WriteLine("Por favor, seleccione otra opción.");

                }

            }

            Console.WriteLine();

        }

    }

}
