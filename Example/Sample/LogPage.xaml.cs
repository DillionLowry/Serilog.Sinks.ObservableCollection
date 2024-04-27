using Serilog;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Sample
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        public LogPage(ObservableCollection<LogEvent> logEvents)
        {
            InitializeComponent();
            DataContext = new { LogEvents = logEvents };
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            switch (random.Next(1, 6))
            {
                case 1:
                    Log.Information("Button was clicked");
                    break;
                case 2:
                    Log.Warning("This is a warning log");
                    break;
                case 3:
                    Log.Debug("This is a debug log");
                    break;
                case 4:
                    try
                    {
                        throw new Exception("This is a test exception");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occurred");
                    }
                    break;
                case 5:
                    Log.Information("Custom formatted log: User {Username} clicked the button", Environment.UserName);
                    break;
            }
        }
    }
}
