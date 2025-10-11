using PizzaShed.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PizzaShed.ViewModels
{
    public class PaymentPresentViewModel : ViewModelBase
    {
        private CheckoutViewModel _checkoutViewModel;


        private string _total;
        public decimal TotalValue
        { 
            get
            {
                if (_total != "")
                    return Convert.ToDecimal(_total);
                return 0;
            }
        }        

        public string Total 
        {
            get => "£" + _total;
            set
            {
                string rawValue = value.StartsWith("£") ? value[1..] : value;                
                SetProperty(ref _total, rawValue);
                OnPropertyChanged(nameof(TotalValue));
            }
        }

        public ICommand ButtonCommand { get; }
        public ICommand MakePaymentCommand { get; }
        public ICommand CancelPaymentCommand { get; }

        public PaymentPresentViewModel(CheckoutViewModel checkoutViewModel)
        {
            _checkoutViewModel = checkoutViewModel;
            _total = _checkoutViewModel.TotalPriceValue.Replace("£", "");

            ButtonCommand = new RelayCommand<string>(ExecuteButtonCommand);
            MakePaymentCommand = new RelayGenericCommand(CheckValidAmount);
            CancelPaymentCommand = new RelayGenericCommand(_checkoutViewModel.CancelPayment);
        }

        private void ExecuteButtonCommand(object? parameter)
        {
            if (parameter is string buttonValue)
            {
                switch (buttonValue)
                {                    
                    case "Clear":
                        Total = "";
                        break;
                    case "Point":
                        if (!Total.Contains('.') && Total.Length > 0)                        
                            Total += ".";
                        break;
                    case "0":
                        if (Total.Contains("."))
                        {
                            if (Total.Split(".")[1].Length < 2)
                                Total += buttonValue;
                        }
                        else
                        {
                            if (Total.Length > 1)
                                Total += buttonValue;
                        }
                        break;
                    default:
                        if (Total.Contains("."))
                        {
                            if (Total.Split(".")[1].Length < 2)
                            {
                                Total += buttonValue;
                            }
                        } else
                        {
                            Total += buttonValue;
                        }
                        break;
                }
            }
        }

        private void CheckValidAmount()
        {
            if (TotalValue != 0)
                _checkoutViewModel.MakePayment(TotalValue);
        }
    }
}
