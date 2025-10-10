using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PizzaShed.Views.Pages
{
    /// <summary>
    /// Interaction logic for PaymentNotPresentView.xaml
    /// </summary>
    public partial class PaymentNotPresentView : UserControl
    {
        public PaymentNotPresentView()
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
