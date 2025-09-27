using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PizzaShed.Services.Data;
using PizzaShed.ViewModel;
using PizzaShed.Helpers;

namespace PizzaShed
{
    /// <summary>
    /// Interaction logic for 
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            // We attach an event handler to the content grid to handle button clicks            
            InitializeComponent();
            var userRepository = new UserRepository();
            DataContext = new LoginViewModel(userRepository);
        }
                
        
        // This function ensures the password box only accepts input from the on-screen keypad 
        private void HandleKeyboardInput(object sender, KeyEventArgs e) 
        {
            e.Handled = true;
        }
    }
}
