using PizzaShed.Model;
using PizzaShed.Services.Logging;
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
        public MainWindow()
        {            
            InitializeComponent();

            Session.Instance.SessionChanged += OnSessionChanged;            
            EventLogger.LogInfo("Application Started");
            UpdateUiForSession();
        }

        private void OnSessionChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateUiForSession();
            });
        }

        private void UpdateUiForSession()
        {
            if (!Session.Instance.IsLoggedIn)
            {
                LoginPage loginPage = new();
                WindowDisplay.Content = loginPage;
            }            
        }
    }
}