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
            GridContent.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ButtonClicked));
        }
        
        private static bool CheckPassword(string pin)
        {
            DatabaseManager instance = DatabaseManager.Instance;            
            User[] users = instance.GetUsers();

            foreach (User user in users)
            {
                if (PasswordHasher.VerifyPin(pin, user.Pin))
                {
                    Session.Instance.Login(user);
                    return true;
                }
            }
            return false;            
        }
        
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            var button= (e.Source as Button);
            
            if (button == null)
            {
                return;
            }
            
            string pin = PinBox.Password;

            switch (button.Name)
            {
                case "Backspace":
                    if (pin.Length > 0)
                    {
                        pin = pin[..^1];
                    }                    
                    break;
                case "Clear":
                    pin = "";
                    break;
                default:
                    pin += button.Content.ToString();
                    break;
            }

            PinBox.Password = pin;

            if (pin.Length == 4)
            {
                if (!CheckPassword(PinBox.Password))
                {
                    ErrorMessage.Text = "Login Failed..";
                    PinBox.Password = "";
                }
            }

            e.Handled = true;
        }
        
        // This function ensures the password box only accepts input from the on-screen keypad 
        private void HandleKeyboardInput(object sender, KeyEventArgs e) 
        {
            e.Handled = true;
        }
    }
}
