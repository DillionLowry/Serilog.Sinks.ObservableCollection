using Serilog.Events;
using Serilog.Parsing;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.ObservableCollection.Tests
{
    public class ObservableCollectionSinkTests
    {
        private static LogEvent TestLog(string message, LogEventLevel level = LogEventLevel.Information)
        {
            return new LogEvent(DateTimeOffset.Now, level, null, new MessageTemplate([new TextToken(message)]), []);
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);

            sink.Emit(logEvent);

            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldAddLogEventsToCollection()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);
            var batch = new List<LogEvent> { logEvent, logEvent };

            await sink.EmitBatchAsync(batch);

            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
            Assert.Equal("Test message", logEvents[1].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldRemoveOldestLogEventWhenMaxStoredEventsIsReached()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 1 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent1 = TestLog("Test message 1", LogEventLevel.Information);
            var logEvent2 = TestLog("Test message 2", LogEventLevel.Information);

            sink.Emit(logEvent1);
            sink.Emit(logEvent2);

            Assert.Single(logEvents);
            Assert.Equal("Test message 2", logEvents[0].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldNotAddNullLogEventToCollection()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            sink.Emit(null!);

            Assert.Empty(logEvents);
        }

        [Fact]
        public void Dispose_ShouldClearLogEvents()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);
            sink.Emit(logEvent);

            sink.Dispose();

            Assert.Empty(logEvents);
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenBatchingIsDisabled()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { EnableBatching = false };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);

            sink.Emit(logEvent);

            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldAddLogEventsToCollection_WhenBatchingIsEnabled()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { EnableBatching = true };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);
            var batch = new List<LogEvent> { logEvent, logEvent };

            await sink.EmitBatchAsync(batch);

            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
            Assert.Equal("Test message", logEvents[1].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldNotAddLogEventToCollection_WhenLogEventLevelIsBelowMinimumLevel()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Information);

            sink.Emit(logEvent);

            Assert.Empty(logEvents);
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenLogEventLevelIsEqualToMinimumLevel()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Warning);

            sink.Emit(logEvent);

            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public void Emit_ShouldAddLogEventToCollection_WhenLogEventLevelIsAboveMinimumLevel()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test message", LogEventLevel.Error);

            sink.Emit(logEvent);

            Assert.Single(logEvents);
            Assert.Equal("Test message", logEvents[0].RenderMessage());
        }

        [Fact]
        public void ShouldThrowException_WhenLogEventsIsNull()
        {
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(null!, dispatcher, options));
        }

        [Fact]
        public void ShouldThrowException_WhenDispatcherIsNull()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(logEvents, null!, options));
        }

        [Fact]
        public void ShouldThrowException_WhenOptionsIsNull()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();

            Assert.Throws<ArgumentNullException>(() => new ObservableCollectionSink(logEvents, dispatcher, options: null!));
        }

        [Fact]
        public void Dispose_ShouldBeIdempotent()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");
            sink.Emit(logEvent);

            // Act & Assert - should not throw
            sink.Dispose();
            sink.Dispose();
            Assert.Empty(logEvents);
        }

#if FEATURE_ASYNCDISPOSABLE
        [Fact]
        public async Task DisposeAsync_ShouldClearLogEvents()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");
            sink.Emit(logEvent);

            await sink.DisposeAsync();

            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task DisposeAsync_ShouldBeIdempotent()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");
            sink.Emit(logEvent);

            // Act & Assert - should not throw
            await sink.DisposeAsync();
            await sink.DisposeAsync();
            Assert.Empty(logEvents);
        }
#endif

        [Fact]
        public void Emit_ShouldNotDispatchAfterDisposal()
        {
            var dispatchCount = 0;
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action =>
            {
                dispatchCount++;
                action();
            };
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");

            sink.Dispose();
            int dispatchesBeforeEmit = dispatchCount;

            sink.Emit(logEvent);

            Assert.Equal(dispatchesBeforeEmit, dispatchCount);
            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldNotDispatchAfterDisposal()
        {
            var dispatchCount = 0;
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action =>
            {
                dispatchCount++;
                action();
            };
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");

            sink.Dispose();
            int dispatchesBeforeEmit = dispatchCount;

            await sink.EmitBatchAsync(new[] { logEvent });

            Assert.Equal(dispatchesBeforeEmit, dispatchCount);
            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldFilterOutNullEventsInBatch()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");
            var batch = new List<LogEvent> { null!, logEvent, null!, logEvent, null! };

            await sink.EmitBatchAsync(batch);

            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Test", logEvents[0].RenderMessage());
            Assert.Equal("Test", logEvents[1].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldFilterOutEventsBeforeMinimumLevelInBatch()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Warning };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var infoEvent = TestLog("Info", LogEventLevel.Information);
            var warningEvent = TestLog("Warning", LogEventLevel.Warning);
            var errorEvent = TestLog("Error", LogEventLevel.Error);
            var batch = new List<LogEvent> { infoEvent, warningEvent, errorEvent, infoEvent };

            await sink.EmitBatchAsync(batch);

            Assert.Equal(2, logEvents.Count);
            Assert.Equal("Warning", logEvents[0].RenderMessage());
            Assert.Equal("Error", logEvents[1].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldIgnoreNullBatch()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            await sink.EmitBatchAsync(null!);

            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldIgnoreEmptyBatch()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            await sink.EmitBatchAsync(new List<LogEvent>());

            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldIgnoreBatchWithOnlyNullOrFilteredEvents()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MinimumLevel = LogEventLevel.Error };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var infoEvent = TestLog("Info");
            var batch = new List<LogEvent> { null!, infoEvent, null! };

            await sink.EmitBatchAsync(batch);

            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task OnEmptyBatchAsync_ShouldInvokeActionWhenConfigured()
        {
            var actionInvoked = false;
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions
            {
                OnEmptyBatchAction = () => actionInvoked = true
            };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            await sink.OnEmptyBatchAsync();

            Assert.True(actionInvoked);
        }

        [Fact]
        public async Task OnEmptyBatchAsync_ShouldHandleNullAction()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions
            {
                OnEmptyBatchAction = null
            };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            await sink.OnEmptyBatchAsync();
        }

        [Fact]
        public void Emit_ShouldPreserveEventOrderWhenMaxStoredEventsExceeded()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 3 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);

            sink.Emit(TestLog("Event 1"));
            sink.Emit(TestLog("Event 2"));
            sink.Emit(TestLog("Event 3"));
            sink.Emit(TestLog("Event 4"));
            sink.Emit(TestLog("Event 5"));

            Assert.Equal(3, logEvents.Count);
            Assert.Equal("Event 3", logEvents[0].RenderMessage());
            Assert.Equal("Event 4", logEvents[1].RenderMessage());
            Assert.Equal("Event 5", logEvents[2].RenderMessage());
        }

        [Fact]
        public void ShouldThrowException_WhenMaxStoredEventsIsSetToZero()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.MaxStoredEvents = 0);
        }

        [Fact]
        public void ShouldThrowException_WhenMaxStoredEventsIsSetToNegative()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.MaxStoredEvents = -1);
        }

        [Fact]
        public void ShouldThrowException_WhenBatchSizeLimitIsSetToZero()
        {
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.BatchSizeLimit = 0);
        }

        [Fact]
        public void ShouldThrowException_WhenBatchSizeLimitIsSetToNegative()
        {
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.BatchSizeLimit = -1);
        }

        [Fact]
        public void ShouldThrowException_WhenPeriodIsSetToZero()
        {
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.Period = TimeSpan.Zero);
        }

        [Fact]
        public void ShouldThrowException_WhenPeriodIsSetToNegative()
        {
            var options = new ObservableCollectionSinkOptions();

            Assert.Throws<ArgumentException>(() => options.Period = TimeSpan.FromMilliseconds(-1));
        }

        [Fact]
        public void Emit_ShouldHandleDispatcherException()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            var exceptionThrown = new InvalidOperationException("Dispatcher error");
            Action<Action> dispatcher = action =>
            {
                throw exceptionThrown;
            };
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");

            Assert.Throws<InvalidOperationException>(() => sink.Emit(logEvent));
            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldHandleDispatcherException()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            var exceptionThrown = new InvalidOperationException("Dispatcher error");
            Action<Action> dispatcher = action =>
            {
                throw exceptionThrown;
            };
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent = TestLog("Test");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sink.EmitBatchAsync(new[] { logEvent }));
            Assert.Equal("Dispatcher error", ex.Message);
            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task ConcurrentEmits_ShouldBeSafe()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 1000 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var tasks = new List<Task>();
            const int threadCount = 10;
            const int emitsPerThread = 100;

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < emitsPerThread; j++)
                    {
                        var logEvent = new LogEvent(
                            DateTimeOffset.Now,
                            LogEventLevel.Information,
                            null,
                            new MessageTemplate($"T{threadId}-E{j}", new List<MessageTemplateToken>()),
                            new List<LogEventProperty>());
                        sink.Emit(logEvent);
                    }
                }, TestContext.Current.CancellationToken));
            }

            await Task.WhenAll(tasks);

            Assert.Equal(threadCount * emitsPerThread, logEvents.Count);
        }

        [Fact]
        public async Task DisposeAsync_ShouldBeThreadSafe()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(sink.DisposeAsync().AsTask());
            }

            await Task.WhenAll(tasks);
            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task Dispose_ShouldBeThreadSafe()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(sink.Dispose, TestContext.Current.CancellationToken));
            }

            await Task.WhenAll(tasks);

            Assert.Empty(logEvents);
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldBeAllOrNothing()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            var callCount = 0;
            Action<Action> dispatcher = action =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("First batch failed");
                }
                action();
            };
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent1 = TestLog("Event 1");
            var logEvent2 = TestLog("Event 2");

            await Assert.ThrowsAsync<InvalidOperationException>(() => sink.EmitBatchAsync(new[] { logEvent1, logEvent2 }));

            Assert.Empty(logEvents);
        }

        [Fact]
        public void Emit_ShouldHandleMaxStoredEventsOfOne()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 1 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent1 = TestLog("Event 1");
            var logEvent2 = TestLog("Event 2");

            sink.Emit(logEvent1);
            sink.Emit(logEvent2);

            Assert.Single(logEvents);
            Assert.Equal("Event 2", logEvents[0].RenderMessage());
        }

        [Fact]
        public async Task EmitBatchAsync_ShouldHandgLargePayload()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { MaxStoredEvents = 10000 };
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var batch = new List<LogEvent>();

            for (int i = 0; i < 1000; i++)
            {
                var logEvent = new LogEvent(
                    DateTimeOffset.Now,
                    LogEventLevel.Information,
                    null,
                    new MessageTemplate($"Event {i}", new List<MessageTemplateToken>()),
                    new List<LogEventProperty>());
                batch.Add(logEvent);
            }

            await sink.EmitBatchAsync(batch);

            Assert.Equal(1000, logEvents.Count);
        }

        [Fact]
        public void ShouldThrowException_WhenConstructorGivenNegativeRetryTimeLimit()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions { RetryTimeLimit = TimeSpan.FromMilliseconds(-1) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new ObservableCollectionSink(logEvents, dispatcher, options));
        }

        [Fact]
        public void EmitAfterDisposeAndEmit_ShouldNotAddToCollection()
        {
            var logEvents = new ObservableCollection<LogEvent>();
            Action<Action> dispatcher = action => action();
            var options = new ObservableCollectionSinkOptions();
            var sink = new ObservableCollectionSink(logEvents, dispatcher, options);
            var logEvent1 = TestLog("Event 1");
            var logEvent2 = TestLog("Event 2");

            sink.Emit(logEvent1);
            Assert.Single(logEvents);

            sink.Dispose();
            Assert.Empty(logEvents);

            sink.Emit(logEvent2);
            Assert.Empty(logEvents);
        }
    }
}