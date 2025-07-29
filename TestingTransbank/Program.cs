using Microsoft.Extensions.DependencyInjection;
using TestingTransbank.Managers;
using TestingTransbank.Repositories;
using TestingTransbank.Services;

class Program
{

    static async Task Main(string[] args)
    {
        
        // 1. Crear el contenedor de servicios
        var services = new ServiceCollection();

        #region Managers
        services.AddSingleton<PrinterManager>();

        services.AddSingleton<DatabaseManager>();
        
        services.AddSingleton<CAFManager>();

        services.AddSingleton<VariableManager>();
        
        services.AddSingleton<POSManager>();
        #endregion

        #region Services

        services.AddSingleton<PrinterService>();

        services.AddSingleton<DatabaseService>();

        services.AddSingleton<VariableService>();

        services.AddSingleton<POSService>();

        services.AddSingleton<TicketService>();

        #endregion

        #region Repositories

        services.AddSingleton<PrinterRepository>();

        services.AddSingleton<CAFService>();

        services.AddSingleton<CAFRepository>();

        services.AddSingleton<VariableRepository>();

        services.AddSingleton<TicketRepository>();

        #endregion

        services.AddTransient<App>(); // La clase principal que usa todo

        // 3. Construir el ServiceProvider
        var provider = services.BuildServiceProvider();

        // 4. Ejecutar la app principal
        var app = provider.GetRequiredService<App>();
        
        await app.Run();
    
    }

}
