using PizzaShed.Services.Data;
using PizzaShed.Model;
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

            IDatabaseManager databaseManager = DatabaseManager.Instance;
            ISession session = new Session();

            IUserRepository userRepository = new UserRepository(databaseManager);
            IProductRepository<Product> productRepository = new ProductRepository(databaseManager);
            IProductRepository<Topping> toppingRepository = new ToppingRepository(databaseManager);
            IOrderRepository orderRepository = new OrderRepository(databaseManager);
            ICustomerRepository customerRepository = new CustomerRepository(databaseManager);
            

            MainWindow window = new()
            {
                DataContext = new MainViewModel(session, userRepository, productRepository, toppingRepository, orderRepository, customerRepository)
            };

            window.Show();
        }
    }

}
