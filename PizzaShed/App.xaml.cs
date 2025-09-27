using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Views.Windows;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PizzaShed
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IUserRepository userRepository = new UserRepository();
            ISession session = new Session();

            MainWindow window = new()
            {
                DataContext = new MainViewModel(userRepository, session)
            };

            window.Show();
        }
    }

}
