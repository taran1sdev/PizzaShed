using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.ViewModels
{
    public class PaymentNotPresentViewModel : ViewModelBase
    {
        private ViewModelBase _checkoutViewModel;
        public PaymentNotPresentViewModel(ViewModelBase checkoutViewModel) 
        { 
            _checkoutViewModel= checkoutViewModel;
        }
    }
}
