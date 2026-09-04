using Serilog.Events;

namespace Serilog.Sinks.ObservableCollection.Tests
{
    public class ObservableCollectionSinkOptionsTests
    {
        [Fact]
        public void ShouldSetDefaultValues()
        {
            var options = new ObservableCollectionSinkOptions();

            Assert.Equal(1000, options.MaxStoredEvents);
            Assert.Equal(LogEventLevel.Verbose, options.MinimumLevel);
            Assert.False(options.EnableBatching);
            Assert.Equal(50, options.BatchSizeLimit);
            Assert.Equal(TimeSpan.FromSeconds(2), options.Period);
        }

        [Fact]
        public void ShouldAllowValuesToBeChanged()
        {
            var options = new ObservableCollectionSinkOptions();

            options.MaxStoredEvents = 500;
            options.MinimumLevel = LogEventLevel.Warning;
            options.EnableBatching = true;
            options.BatchSizeLimit = 100;
            options.Period = TimeSpan.FromSeconds(1);

            Assert.Equal(500, options.MaxStoredEvents);
            Assert.Equal(LogEventLevel.Warning, options.MinimumLevel);
            Assert.True(options.EnableBatching);
            Assert.Equal(100, options.BatchSizeLimit);
            Assert.Equal(TimeSpan.FromSeconds(1), options.Period);
        }

        [Fact]
        public void ShouldThrowException_WhenConstructorGivenBatchSizeLimitOfZero()
        {
            Assert.Throws<ArgumentException>(() => new ObservableCollectionSinkOptions { BatchSizeLimit = 0 });
        }

        [Fact]
        public void ShouldThrowException_WhenConstructorGivenPeriodOfZero()
        {
            Assert.Throws<ArgumentException>(() => new ObservableCollectionSinkOptions { Period = TimeSpan.Zero });
        }
    }
}