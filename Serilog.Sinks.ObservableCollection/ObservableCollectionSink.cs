using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.ObservableCollection;

/// <summary>
/// A Serilog sink that writes log events to an <see cref="ObservableCollection{LogEvent}"/>.
/// </summary>
/// <remarks>
/// This sink can optionally use the batching functionality provided by the <see cref="Serilog.Sinks.PeriodicBatching"/> package.
/// When batching is enabled, log events are collected over a period of time and then dispatched in batches to the observable collection.
/// If no logs were generated within the spefified batch time, <see cref="OnEmptyBatchAsync"/> is called.
/// </remarks>
public class ObservableCollectionSink : ILogEventSink, IBatchedLogEventSink, IDisposable
{
    private readonly Action<Action> _dispatcher;
    private readonly ObservableCollectionSinkOptions _options;

    public ObservableCollection<LogEvent> LogEvents { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollectionSink"/> class.
    /// </summary>
    /// <param name="logEvents">The observable collection to write log events to.</param>
    /// <param name="dispatcher">The dispatcher to use for dispatching log events to the observable collection.</param>
    /// <param name="options">The options for the observable collection sink.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the arguments are null.</exception>
    public ObservableCollectionSink(ObservableCollection<LogEvent> logEvents, Action<Action> dispatcher, ObservableCollectionSinkOptions options)
    {
        LogEvents = logEvents ?? throw new ArgumentNullException(nameof(logEvents));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Emits a log event to the sink.
    /// </summary>
    /// <param name="logEvent">The log event to emit.</param>
    public void Emit(LogEvent logEvent)
    {
        // If Batching is enabled handled, Emit is handled by PeriodicBatchingSink
        if (logEvent == null || logEvent.Level < _options.MinimumLevel || _options.EnableBatching)
        {
            return;
        }

        ProcessLogEvent(logEvent);
    }

    /// <summary>
    /// Emits a batch of log events to the sink asynchronously.
    /// </summary>
    /// <param name="batch">The batch of log events to emit.</param>
    /// <returns>A task that represents the asynchronous emit operation.</returns>
    public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        foreach (var logEvent in batch)
        {
            ProcessLogEvent(logEvent);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs any periodic work required when no events have been emitted for some time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnEmptyBatchAsync()
    {
        _options.OnEmptyBatchAction?.Invoke();
        return Task.CompletedTask;
    }

    private void ProcessLogEvent(LogEvent logEvent)
    {
        if (logEvent == null) return;
        _dispatcher(() =>
        {
            if (LogEvents.Count >= _options.MaxStoredEvents)
                LogEvents.RemoveAt(0);

            LogEvents.Add(logEvent);
        });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        LogEvents.Clear();
    }
}