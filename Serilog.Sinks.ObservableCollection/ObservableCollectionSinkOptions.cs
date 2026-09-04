using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.ObservableCollection;

public class ObservableCollectionSinkOptions
{
    private int _maxStoredEvents = 1000;
    private int _batchSizeLimit = 50;
    private TimeSpan _period = TimeSpan.FromSeconds(2);

    public ObservableCollectionSinkOptions()
    {
    }

    public ObservableCollectionSinkOptions(BatchingOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        Period = options.BufferingTimeLimit;
        RetryTimeLimit = options.RetryTimeLimit;
        BatchSizeLimit = options.BatchSizeLimit;
        EagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;
        QueueLimit = options.QueueLimit;
        EnableBatching = true;
    }

    /// <summary>
    /// Gets or sets the action to be performed when no events have been emitted for a <see
    /// cref="Period"/> of time.
    /// </summary>
    /// <remarks>Only invoked when <see cref="EnableBatching"/> is <c>true</c>.</remarks>
    public Action? OnEmptyBatchAction { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of events to store in the observable collection.
    /// </summary>
    /// <remarks>
    /// When this limit is reached, the oldest event is removed when a new event is added. Must be
    /// greater than 0.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when set to a value less than or equal to 0.
    /// </exception>
    public int MaxStoredEvents
    {
        get => _maxStoredEvents;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("MaxStoredEvents must be greater than 0.", nameof(value));
            }

            _maxStoredEvents = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum time to wait for a single event's dispatcher callback to complete
    /// during batched emission before abandoning it and reporting the batch as failed.
    /// </summary>
    /// <remarks>
    /// Guards against a deadlock where the dispatcher's target thread (e.g. a UI thread) is itself
    /// blocked waiting for Serilog's batching pipeline to finish disposing. Only relevant when <see
    /// cref="EnableBatching"/> is <c>true</c>. Null disables the timeout and restores blocking,
    /// unbounded dispatch.
    /// </remarks>
    public TimeSpan? DispatchTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the maximum time to wait between retries after a batch dispatch failure before
    /// giving up and dropping queued events. Null uses Serilog's default.
    /// </summary>
    /// <remarks>Only relevant when <see cref="EnableBatching"/> is <c>true</c>.</remarks>
    public TimeSpan? RetryTimeLimit { get; set; }

    /// <summary>
    /// Gets or sets the minimum log event level to include in the collection.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Verbose;

    /// <summary>
    /// Gets or sets whether events are buffered and dispatched via Serilog's native
    /// <c>BatchingOptions</c> pipeline instead of individually as they're logged.
    /// </summary>
    public bool EnableBatching { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of log events to include in each batch.
    /// </summary>
    /// <remarks>Must be greater than 0. Only relevant when <see cref="EnableBatching"/> is <c>true</c>.</remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when set to a value less than or equal to 0.
    /// </exception>
    public int BatchSizeLimit
    {
        get => _batchSizeLimit;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("BatchSizeLimit must be greater than 0.", nameof(value));
            }

            _batchSizeLimit = value;
        }
    }

    /// <summary>
    /// Gets or sets the time period between batch emissions.
    /// </summary>
    /// <remarks>
    /// Must be a positive TimeSpan. Only relevant when <see cref="EnableBatching"/> is <c>true</c>.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when set to a non-positive TimeSpan.</exception>
    public TimeSpan Period
    {
        get => _period;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("Period must be greater than zero.", nameof(value));
            }

            _period = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the first event is emitted immediately rather than waiting for the
    /// first <see cref="Period"/> to elapse.
    /// </summary>
    /// <remarks>Only relevant when <see cref="EnableBatching"/> is <c>true</c>.</remarks>
    public bool EagerlyEmitFirstEvent { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of events held in the internal buffer awaiting dispatch,
    /// beyond which new events are dropped.
    /// </summary>
    /// <remarks>Null means unbounded. Only relevant when <see cref="EnableBatching"/> is <c>true</c>.</remarks>
    public int? QueueLimit { get; set; }
}