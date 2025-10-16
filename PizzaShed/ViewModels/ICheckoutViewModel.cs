using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.ViewModels
{
    public interface ICheckoutViewModel
    {
        public string TotalPriceValue { get; }

        public void CancelPayment();
        public void MakePayment(decimal amount);
    }
}
