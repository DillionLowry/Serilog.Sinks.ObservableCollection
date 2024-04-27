# Serilog.Sinks.ObservableCollection
The `ObservableCollectionSink` is a custom Serilog sink designed to log messages to an `ObservableCollection`, which can be particularly useful in WPF applications where logs need to be displayed in the UI in real-time.
## Prerequisites

- Serilog library installed in your .NET project.
- Access to a dispatcher (typically from a UI context in WPF or similar).
- Optional: Serilog.Sinks.PeriodicBatching for batch processing.

## Installation

Ensure Serilog is installed in your project:
```bash
Install-Package Serilog
```
For batching capabilities, install the PeriodicBatching package:

```bash
Install-Package Serilog.Sinks.PeriodicBatching
```
## Configuration

Configure the sink within your application's logging setup. Examples are provided for both standard and batched logging setups.
### Standard Logging Configuration

```csharp
var LogEvents = new ObservableCollection<LogEventViewModel>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.ObservableCollection(
        LogEvents,
        dispatcher: action => Application.Current.Dispatcher.Invoke,
        configure: options => 
        {
            options.MaxStoredEvents = 1000;  // Maximum log events to store
            options.MinimumLevel = LogEventLevel.Information;
        })
    .CreateLogger();
```
### Batched Logging Configuration

Enable batching by configuring the sink accordingly:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.ObservableCollection(
        LogEvents,
        dispatcher: action => dispatcher.Invoke(action),
        configure: options => 
        {
            options.EnableBatching = true;
            options.BatchSizeLimit = 50;
            options.Period = TimeSpan.FromSeconds(2);
            options.MaxStoredEvents = 1000;
            options.OnEmptyBatchAction = async () =>
            {
                // Custom action to perform when no events have been emitted
                await Task.Run(() => Debug.WriteLine("No events emitted for the specified period."));
            };
        })
    .CreateLogger();
```

### Dependency Injection Configuration
```csharp
_host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ObservableCollection<LogEvent>>();
        // other services
    })
    .UseSerilog((context, services, configuration) =>
    {
        var logEvents = services.GetRequiredService<ObservableCollection<LogEventViewModel>>();
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
```
## Usage

Once configured, use the sink like any other Serilog sink:

```csharp
Log.Information("This is a test log message.");
```

Examples of viewing logs in a WPF application are provided in the sample project.

## Disposal

Remember to properly dispose of your loggers when the application is closing:

```csharp
Log.CloseAndFlush();
```
This is especially important in batched mode to ensure all pending log messages are flushed to the UI.
## Additional Notes

- Ensure the dispatcher is suitable for the thread managing the UI component to avoid cross-thread operations.
- MaxStoredEvents helps manage memory by limiting the number of log events stored. Adjust based on performance needs.