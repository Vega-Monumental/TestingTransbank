using Microsoft.Extensions.Configuration;
using TestingTransbank.Models;

namespace TestingTransbank.Helpers
{

    public class ConfigurationHelper
    {

        private static IConfigurationRoot _configuration;

        static ConfigurationHelper()
        {

            _configuration = new ConfigurationBuilder()
                
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

                .Build();

        }

        public static string SelectedPrinter => _configuration["SelectedPrinter"];

        public static void SetSelectedPrinter(string printer)
        {

            _configuration["SelectedPrinter"] = printer;

        }

        public static string SelectedPort => _configuration["SelectedPort"];

        public static void SetSelectedPort(string port)
        {

            _configuration["SelectedPort"] = port;

        }

        public static string SelectedDatabase => _configuration["SelectedDatabase"];

        public static void SetSelectedDatabase(string database)
        {

            _configuration["SelectedDatabase"] = database;

        }

        public static string SelectedConnectionString => _configuration["SelectedConnectionString"];

        public static void SetSelectedConnectionString(string connectionString)
        {

            _configuration["SelectedConnectionString"] = connectionString;

        }

        public static List<DatabaseInfo> AvailableDatabases => _configuration.GetSection("AvailableDatabases").Get<List<DatabaseInfo>>();

    }

}
