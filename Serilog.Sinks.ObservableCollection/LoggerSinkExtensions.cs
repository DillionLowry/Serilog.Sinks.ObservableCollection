using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.ObservableCollection;
using Serilog.Sinks.PeriodicBatching;
using System.Collections.ObjectModel;

namespace Serilog;

/// <summary>
/// Provides extension methods on <see cref="LoggerConfiguration"/> to configure an <see cref="ObservableCollection{LogEvent}"/> sink.
/// </summary>
///     /// <remarks>
/// This sink can optionally use the batching functionality provided by the <see cref="Serilog.Sinks.PeriodicBatching"/> package.
/// When batching is enabled, log events are collected over a period of time and then dispatched in batches to the observable collection.
/// This can improve performance when logging a high number of events.
/// </remarks>
public static class LoggerSinkExtensions
{
    /// <summary>
    /// Adds a sink that writes log events to an <see cref="ObservableCollection{LogEvent}"/>.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="logEvents">The observable collection to write log events to.</param>
    /// <param name="dispatcher">The dispatcher to use for dispatching log events to the observable collection.</param>
    /// <param name="configure">An action to configure the options for the observable collection sink.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <remarks>
    /// This method allows log events to be observed in real-time. It can be useful in scenarios where you want to display log events in the UI.
    /// The dispatcher parameter is used to control how log events are dispatched to the observable collection. It's typically used to dispatch log events to the UI thread in desktop applications.
    /// If batching is enabled in the options, this method uses the <see cref="PeriodicBatchingSink"/> from the <see cref="Serilog.Sinks.PeriodicBatching"/> package to collect and dispatch log events in batches.
    /// </remarks>
    public static LoggerConfiguration ObservableCollection(
            this LoggerSinkConfiguration loggerConfiguration,
            ObservableCollection<LogEvent> logEvents,
            Action<Action> dispatcher,
            Action<ObservableCollectionSinkOptions> configure)
    {
        var options = new ObservableCollectionSinkOptions();

        configure?.Invoke(options);

        if (options.EnableBatching)
        {
            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = options.BatchSizeLimit,
                Period = options.Period
            };
            return loggerConfiguration.Sink(new PeriodicBatchingSink(new ObservableCollectionSink(logEvents, dispatcher, options), batchingOptions), restrictedToMinimumLevel: options.MinimumLevel);
        }
        else
        {
            return loggerConfiguration.Sink(new ObservableCollectionSink(logEvents, dispatcher, options));
        }
    }
}