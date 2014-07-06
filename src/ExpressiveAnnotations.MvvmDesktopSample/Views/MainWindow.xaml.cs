using System.Windows;
using ExpressiveAnnotations.MvvmDesktopSample.Models;

namespace ExpressiveAnnotations.MvvmDesktopSample.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowVM
            {
                Query = new QueryVM
                {
                    ContactDetails = new ContactVM()
                }
            };
            InitializeComponent();
        }
    }
}
