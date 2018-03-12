using System;
using System.IO;
using System.Threading;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace ProductContext.WebApi
{
    internal static class LoggingHelper
    {
        private const string CaptureCorrelationIdKey = "CaptureCorrelationId";
        private static readonly Subject<LogEvent> s_logEventSubject = new Subject<LogEvent>();

        private static readonly MessageTemplateTextFormatter s_formatter = new MessageTemplateTextFormatter(
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}",
            null);

        private static readonly MessageTemplateTextFormatter s_formatterWithException = new MessageTemplateTextFormatter(
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
            null);

        static LoggingHelper()
        {
            Log.Logger = new LoggerConfiguration()
                         .WriteTo
                         .Observers(observable => observable.Subscribe(@event => s_logEventSubject.OnNext(@event)))
                         .Enrich.FromLogContext()
                         .MinimumLevel.Verbose()
                         .CreateLogger();
        }

        public static IDisposable Capture(TextWriter outputHelper)
        {
            var captureId = Guid.NewGuid();

            var callContextData = new AsyncLocal<Tuple<string, Guid>>
            {
                Value = new Tuple<string, Guid>(CaptureCorrelationIdKey, captureId)
            };

            Func<LogEvent, bool> filter = logEvent => callContextData.Value.Item2.Equals(captureId);

            IDisposable subscription = s_logEventSubject.Where(filter).Subscribe(logEvent =>
            {
                using (var writer = new StringWriter())
                {
                    if (logEvent.Exception != null)
                    {
                        s_formatterWithException.Format(logEvent, writer);
                    }
                    else
                    {
                        s_formatter.Format(logEvent, writer);
                    }

                    outputHelper.WriteLine(writer.ToString());
                }
            });

            return new DisposableAction(() =>
            {
                subscription.Dispose();
                callContextData.Value = null;
            });
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;

            public DisposableAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}
