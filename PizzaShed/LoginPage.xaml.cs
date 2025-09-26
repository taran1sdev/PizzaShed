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
            Content.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonClicked));
        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            var button= (e.Source as Button);
            string password = PinBox.Password;

            switch (button.Name)
            {
                case "Backspace":
                    if (password.Length > 0)
                    {
                        password = password.Remove(password.Length - 1);
                    }                    
                    break;
                case "Clear":
                    password = "";
                    break;
                default:
                    password += button.Content.ToString();
                    break;
            }

            PinBox.Password = password;
        }
        
        // This function ensures the password box only accepts input from the on-screen keypad 
        private void HandleKeyboardInput(object sender, KeyEventArgs e) 
        {
            e.Handled = true;
        }
    }
}
