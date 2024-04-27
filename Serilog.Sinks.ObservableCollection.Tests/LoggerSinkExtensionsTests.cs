using Serilog.Events;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.ObservableCollection.Tests
{
    public class LoggerSinkExtensionsTests
    {
        [Fact]
        public void ShouldThrowException_WhenLogEventsIsNull()
        {
            // Arrange
            Action<Action> dispatcher = action => action();
            var loggerConfiguration = new LoggerConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => loggerConfiguration.WriteTo.ObservableCollection(null, dispatcher, options => { }));
        }

        [Fact]
        public void ShouldThrowException_WhenDispatcherIsNull()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            var options = new ObservableCollectionSinkOptions();
            var loggerConfiguration = new LoggerConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => loggerConfiguration.WriteTo.ObservableCollection(logEvents, null, options => { }));
        }
    }
}