using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Sample;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<ObservableCollection<LogEvent>>();
                services.AddTransient<LogPage>();
            })
            .UseSerilog((context, services, configuration) =>
            {
                var logEvents = services.GetRequiredService<ObservableCollection<LogEvent>>();
                configuration
                    .MinimumLevel.Debug() // Set the global minimum level to Debug
                    .WriteTo.ObservableCollection(logEvents, Application.Current.Dispatcher.Invoke, options =>
                    {
                        options.MaxStoredEvents = 1000;
                        options.EnableBatching = true;
                        options.BatchSizeLimit = 50;
                        options.Period = TimeSpan.FromSeconds(2);
                        options.MinimumLevel = LogEventLevel.Information; // set the minimum level for the ObservableCollection sink to Information
                        options.OnEmptyBatchAction = async () =>
                        {
                            // Custom action to perform when no events have been emitted for a certain period of time
                            await Task.Run(() => Debug.WriteLine("No events emitted for the specified period."));
                        };
                    });
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        mainWindow.Navigate(_host.Services.GetService<LogPage>());

        // Example usage of the logger
        Log.Information("Application started");
        Log.Debug("This is a debug message");
        Log.Warning("This is a warning message");
        Log.Error("This is an error message");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await Serilog.Log.CloseAndFlushAsync();
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}