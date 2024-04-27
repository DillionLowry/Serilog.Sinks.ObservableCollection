using Serilog.Events;
using System.Globalization;
using System.Windows.Data;

namespace Sample
{
    public class LogEventMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogEvent logEvent)
            {
                return logEvent.RenderMessage();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}