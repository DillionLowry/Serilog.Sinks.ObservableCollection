using System.Windows;
using System.Windows.Controls;

namespace Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool Navigate(Page page)
        {
            return this.MainNavigationFrame.Navigate(page);
        }
    }
}