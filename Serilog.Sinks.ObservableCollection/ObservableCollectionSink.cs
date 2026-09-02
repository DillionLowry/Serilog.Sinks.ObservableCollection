using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.ObservableCollection;

/// <summary>
/// A Serilog sink that writes log events to an <see cref="ObservableCollection{LogEvent}"/>.
/// Implements both <see cref="ILogEventSink"/> and <see cref="IBatchedLogEventSink"/> and can be
/// wired up directly or through Serilog's native <see cref="BatchingOptions"/> pipeline.
/// </summary>
/// <remarks>
/// If no logs were generated within the specified batch time, <see cref="OnEmptyBatchAsync"/> is
/// called. When batching is enabled, prefer disposing the logger asynchronously to avoid a
/// potential deadlock if the sink's dispatcher marshals onto the same thread that performs disposal.
/// </remarks>
public sealed class ObservableCollectionSink : ILogEventSink, IBatchedLogEventSink, IDisposable
#if FEATURE_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
{
    private readonly Action<Action> _dispatcher;
    private readonly ObservableCollectionSinkOptions _options;
    private volatile bool _disposed;
#if NET8_0_OR_GREATER
    private readonly Lock _stateLock = new Lock();
#else
    private readonly object _stateLock = new object();
#endif

    public ObservableCollection<LogEvent> LogEvents { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollectionSink"/> class.
    /// </summary>
    /// <param name="logEvents">The observable collection to store log events.</param>
    /// <param name="dispatcher">The action used to dispatch log events.</param>
    /// <param name="options">The options for configuring the sink.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the options have invalid values.
    /// </exception>
    public ObservableCollectionSink(ObservableCollection<LogEvent> logEvents, Action<Action> dispatcher, ObservableCollectionSinkOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (options.BatchSizeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");
        if (options.Period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The batching period must be greater than zero.");
        if (options.RetryTimeLimit < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The retry time limit must not be negative.");

        if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
        if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

        LogEvents = logEvents;
        _dispatcher = dispatcher;
        _options = options;
    }

    /// <inheritdoc cref="ObservableCollectionSink(ObservableCollection{LogEvent}, Action{Action}, ObservableCollectionSinkOptions)"/>
    public ObservableCollectionSink(ObservableCollection<LogEvent> logEvents, Action<Action> dispatcher, BatchingOptions batchingOptions)
        : this(logEvents, dispatcher, new ObservableCollectionSinkOptions(batchingOptions)) { }

    /// <summary>
    /// Emits a log event to the sink. Only invoked when the sink is configured without batching.
    /// </summary>
    /// <param name="logEvent">The event to add to <see cref="LogEvents"/>. Null events are ignored.</param>
    /// <remarks>
    /// Thread-safe with respect to <see cref="Dispose"/> and <see cref="DisposeAsync"/>: once
    /// disposal starts, no further events will be dispatched. Events below <see
    /// cref="ObservableCollectionSinkOptions.MinimumLevel"/> are dropped without being dispatched.
    /// </remarks>
    public void Emit(LogEvent logEvent)
    {
        // Checking MinimumLevel should be redundant because of Serilog's filtering but is included
        // to handle custom setups where the sink emit is called directly.
        if (_disposed || logEvent is null || logEvent.Level < _options.MinimumLevel)
        {
            return;
        }

        lock (_stateLock)
        {
            if (_disposed) return;
            _dispatcher(() => AddLogEvent(logEvent));
        }
    }

    /// <summary>
    /// Emits a batch of log events. Only invoked when the sink is configured with batching.
    /// </summary>
    /// <param name="batch">
    /// The events to add to <see cref="LogEvents"/>. Null or empty batches are ignored; individual
    /// null events and events below <see cref="ObservableCollectionSinkOptions.MinimumLevel"/> are
    /// filtered out before dispatch.
    /// </param>
    /// <returns>
    /// A task that completes once the whole batch has been added, or faults if dispatch fails or
    /// exceeds <see cref="ObservableCollectionSinkOptions.DispatchTimeout"/>. On failure, none of
    /// the batch's events are added to <see cref="LogEvents"/>. Dispatch is all-or-nothing, so
    /// Serilog's retry of the same batch cannot produce duplicates.
    /// </returns>
    /// <remarks>
    /// The batch is dispatched on a thread-pool thread rather than the calling thread, to allow a
    /// timeout to be applied without blocking Serilog's batching pipeline indefinitely.
    /// </remarks>
    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        if (_disposed || batch is null) return;

        var toAdd = batch.Where(e => e is not null && e.Level >= _options.MinimumLevel).ToArray();
        if (toAdd.Length == 0) return;

        await DispatchBatchAsync(toAdd).ConfigureAwait(false);
    }

    private Task DispatchBatchAsync(LogEvent[] events)
    {
        if (_disposed) return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                _dispatcher(() =>
                {
                    foreach (var e in events)
                        AddLogEvent(e);
                });
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return _options.DispatchTimeout is { } timeout
            ? WaitWithTimeoutAsync(tcs.Task, timeout)
            : tcs.Task;
    }

    /// <summary>
    /// Invokes <see cref="ObservableCollectionSinkOptions.OnEmptyBatchAction"/>, if configured.
    /// Only called when the sink is configured with batching and no events were emitted during a
    /// batch period.
    /// </summary>
    public Task OnEmptyBatchAsync()
    {
        _options.OnEmptyBatchAction?.Invoke();
        return Task.CompletedTask;
    }

    private static async Task WaitWithTimeoutAsync(Task dispatchTask, TimeSpan timeout)
    {
        var completed = await Task.WhenAny(dispatchTask, Task.Delay(timeout)).ConfigureAwait(false);

        if (completed != dispatchTask)
        {
            SelfLog.WriteLine("ObservableCollectionSink: dispatch did not complete within {0}; the target dispatcher thread may be blocked.", timeout);
            throw new TimeoutException($"ObservableCollectionSink dispatch did not complete within {timeout}.");
        }

        await dispatchTask.ConfigureAwait(false);
    }

    private void AddLogEvent(LogEvent logEvent)
    {
        if (LogEvents.Count > 0 && LogEvents.Count >= _options.MaxStoredEvents)
        {
            LogEvents.RemoveAt(0);
        }

        LogEvents.Add(logEvent);
    }

    /// <summary>
    /// Disposes the sink synchronously: marks it as disposed, then clears <see cref="LogEvents"/>
    /// via the dispatcher. Safe to call more than once, subsequent calls are no-ops. Blocks the
    /// calling thread until the dispatcher has run the clear action if the dispatcher itself blocks
    /// (e.g. <c>Dispatcher.Invoke</c>).
    /// </summary>
    /// <remarks>
    /// Reentrancy is not supported: The dispatched action must not synchronously re-enter this
    /// sink's <see cref="Emit"/>, <see cref="Dispose"/>, or <see cref="DisposeAsync"/> from the
    /// same dispatcher call.
    /// </remarks>
    public void Dispose()
    {
        lock (_stateLock)
        {
            if (_disposed) return;
            _disposed = true;
            _dispatcher(() => LogEvents.Clear());
        }
    }

#if FEATURE_ASYNCDISPOSABLE
    /// <summary>
    /// Disposes the sink asynchronously: marks it as disposed, then clears <see cref="LogEvents"/>
    /// via the dispatcher. Prefer this over <see cref="Dispose"/> when batching is enabled to avoid
    /// a potential deadlock if the dispatcher's target thread is also the thread performing
    /// disposal. Safe to call more than once, subsequent calls are no-ops.
    /// </summary>
    /// <remarks>
    /// Reentrancy is not supported: The dispatched action must not synchronously re-enter this
    /// sink's <see cref="Emit"/>, <see cref="Dispose"/>, or <see cref="DisposeAsync"/> from the
    /// same dispatcher call.
    /// </remarks>
    public ValueTask DisposeAsync()
    {
        TaskCompletionSource? tcs;

        lock (_stateLock)
        {
            if (_disposed)
            {
                return new ValueTask(Task.CompletedTask);
            }

            _disposed = true;

            tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _dispatcher(() =>
            {
                LogEvents.Clear();
                tcs.SetResult();
            });
        }

        return new ValueTask(tcs.Task);
    }
#endif
}