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
using PizzaShed.Services.Data;
using PizzaShed.ViewModels;
using PizzaShed.Helpers;

namespace PizzaShed.Views.Pages
{
    /// <summary>
    /// Interaction logic for CashierView.xaml
    /// </summary>
    public partial class CashierView : UserControl
    {
        public CashierView()
        {
            InitializeComponent();

            var ProductRepository = new ProductRepository();
            var ToppingRepository = new ToppingRepository();

            var viewModel = new CashierViewModel(ProductRepository, ToppingRepository);

            this.DataContext = viewModel;
        }
    }
}
