using Serilog.Events;

namespace Serilog.Sinks.ObservableCollection
{
    public class ObservableCollectionSinkOptions
    {
        /// <summary>
        /// Gets or sets the action to be performed when no events have been emitted for a <see cref="Period"/> of time.
        /// </summary>
        public Action? OnEmptyBatchAction { get; set; }
        public int MaxStoredEvents { get; set; } = 1000;
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Verbose;
        public bool EnableBatching { get; set; } = false;
        public int BatchSizeLimit { get; set; } = 50;
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
    }
}