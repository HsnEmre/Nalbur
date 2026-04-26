using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure;
using Nalbur.Infrastructure.Services;
using Nalbur.Wpf.ViewModels;
using Nalbur.Wpf.Views;
using System.Windows;

namespace Nalbur.Wpf;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection") 
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                // Infrastructure
                services.AddInfrastructure(connectionString);
                services.AddScoped<DataSeeder>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProductViewModel>();
                services.AddTransient<CustomerViewModel>();
                services.AddTransient<SalesViewModel>();
                services.AddTransient<InstallmentViewModel>();
                services.AddTransient<SalesHistoryViewModel>();
                services.AddTransient<OutgoingPaymentsViewModel>();
                services.AddScoped<IWorkContractService, WorkContractService>();
                services.AddTransient<WorkContractViewModel>();
                services.AddTransient<WorkContractView>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<ReportsView>();
                // Views
                services.AddSingleton<MainWindow>(s => new MainWindow
                {
                    DataContext = s.GetRequiredService<MainViewModel>()
                });
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        // Seed Data
        using (var scope = AppHost.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}
