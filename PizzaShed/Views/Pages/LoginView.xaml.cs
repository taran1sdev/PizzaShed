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
using PizzaShed.ViewModels;
using PizzaShed.Helpers;

namespace PizzaShed.Views.Pages
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {            
            InitializeComponent();            
        }
                
        
        // This function ensures the password box only accepts input from the on-screen keypad 
        private void HandleKeyboardInput(object sender, KeyEventArgs e) 
        {
            e.Handled = true;
        }
    }
}
