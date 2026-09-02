using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.ObservableCollection;
using System.Collections.ObjectModel;

namespace Serilog;

/// <summary>
/// Provides extension methods on <see cref="LoggerConfiguration"/> to configure an <see
/// cref="ObservableCollection{LogEvent}"/> sink.
/// </summary>
public static class LoggerSinkExtensions
{
    /// <summary>
    /// Adds a sink that writes log events to an <see cref="ObservableCollection{LogEvent}"/>.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="logEvents">The observable collection to write log events to.</param>
    /// <param name="dispatcher">
    /// The dispatcher to use for dispatching log events to the observable collection.
    /// </param>
    /// <param name="configure">
    /// An action to configure the options for the observable collection sink.
    /// </param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <remarks>
    /// The dispatcher parameter is typically used to marshal collection mutations onto the UI
    /// thread in desktop applications. When batching is enabled, Serilog's built-in <see
    /// cref="BatchingOptions"/> pipeline collects and dispatches events in batches.
    /// </remarks>
    public static LoggerConfiguration ObservableCollection(
            this LoggerSinkConfiguration loggerConfiguration,
            ObservableCollection<LogEvent> logEvents,
            Action<Action> dispatcher,
            Action<ObservableCollectionSinkOptions>? configure = null)
    {
        var options = new ObservableCollectionSinkOptions();

        configure?.Invoke(options);

        var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

        if (!options.EnableBatching)
        {
            return loggerConfiguration.Sink(sink, options.MinimumLevel);
        }

        var batchingOptions = new BatchingOptions
        {
            BatchSizeLimit = options.BatchSizeLimit,
            BufferingTimeLimit = options.Period,
            EagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent,
            QueueLimit = options.QueueLimit
        };

        if (options.RetryTimeLimit is { } retryTimeLimit)
        {
            batchingOptions.RetryTimeLimit = retryTimeLimit;
        }

        return loggerConfiguration.Sink(sink, batchingOptions, options.MinimumLevel);
    }

    /// <summary>
    /// Adds a sink that writes log events to an <see cref="ObservableCollection{LogEvent}"/> with batching options.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="logEvents">The observable collection to write log events to.</param>
    /// <param name="dispatcher">The dispatcher to use for dispatching log events to the observable collection.</param>
    /// <param name="batchingOptions">The batching options to use for the sink.</param>
    /// <param name="minimumLevel">The minimum log event level required to write to the sink.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    public static LoggerConfiguration ObservableCollection(
            this LoggerSinkConfiguration loggerConfiguration,
            ObservableCollection<LogEvent> logEvents,
            Action<Action> dispatcher,
            BatchingOptions batchingOptions,
            LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        var sink = new ObservableCollectionSink(logEvents, dispatcher, batchingOptions);
        return loggerConfiguration.Sink(sink, batchingOptions, minimumLevel);
    }
}