using Serilog.Events;
using Serilog.Parsing;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.ObservableCollection.Tests
{
    public class ObservableCollectionSinkTests
    {
        [Fact]
        public void Emit_ShouldAddLogEventToCollection()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, messageTemplate, new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent);

            // Assert
            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public void EmitBatchAsync_ShouldAddLogEventsToCollection()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test message", new List<MessageTemplateToken>() { new TextToken("Test message") }), new List<LogEventProperty>());
            var batch = new List<LogEvent> { logEvent, logEvent };

            // Act
            sink.EmitBatchAsync(batch).Wait();

            // Assert
            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
            Assert.Equal("Test message", logEvents[1].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldRemoveOldestLogEventWhenMaxStoredEventsIsReached()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 1 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent1 = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test message 1", new List<MessageTemplateToken>() { new TextToken("Test message 1") }), new List<LogEventProperty>());
            var logEvent2 = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test message 2", new List<MessageTemplateToken>() { new TextToken("Test message 2") }), new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent1);
            sink.Emit(logEvent2);

            // Assert
            Assert.Single(logEvents);
            Assert.Equal("Test message 2", logEvents[0].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldNotAddNullLogEventToCollection()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            // Act
            sink.Emit(null);

            // Assert
            Assert.Empty(logEvents);
        }

        [Fact]
        public void Dispose_ShouldClearLogEvents()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("Test message", new List<MessageTemplateToken>()), new List<LogEventProperty>());
            sink.Emit(logEvent);

            // Act
            sink.Dispose();

            // Assert
            Assert.Empty(logEvents);
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenBatchingIsDisabled()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { EnableBatching = false };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, messageTemplate, new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent);

            // Assert
            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldAddLogEventsToCollection_WhenBatchingIsEnabled()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { EnableBatching = true };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, messageTemplate, new List<LogEventProperty>());
            var batch = new List<LogEvent> { logEvent, logEvent };

            // Act
            await sink.EmitBatchAsync(batch);

            // Assert
            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
            Assert.Equal("Test message", logEvents[1].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldNotAddLogEventToCollection_WhenLogEventLevelIsBelowMinimumLevel()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, messageTemplate, new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent);

            // Assert
            Assert.Empty(logEvents);
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenLogEventLevelIsEqualToMinimumLevel()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Warning, null, messageTemplate, new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent);

            // Assert
            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenLogEventLevelIsAboveMinimumLevel()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var messageTemplate = new MessageTemplate("Test message", new List<MessageTemplateToken>
            {
                new TextToken("Test message")
            });
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, null, messageTemplate, new List<LogEventProperty>());

            // Act
            sink.Emit(logEvent);

            // Assert
            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public void ShouldThrowException_WhenLogEventsIsNull()
        {
            // Arrange
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(null, dispatcher, options));
        }

        [Fact]
        public void ShouldThrowException_WhenDispatcherIsNull()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            var options = new ObservableCollectionSinkOptions();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(logEvents, null, options));
        }

        [Fact]
        public void ShouldThrowException_WhenOptionsIsNull()
        {
            // Arrange
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(logEvents, dispatcher, null));
        }
    }
}