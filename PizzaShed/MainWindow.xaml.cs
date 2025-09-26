using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PizzaShed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginPage LoginPage { get; set; }
        public MainWindow()
        {            
            InitializeComponent();
            EventLogger.LogInfo("Application Started");
            LoginPage = new LoginPage();
            WindowDisplay.Content = LoginPage;
        }
    }
}