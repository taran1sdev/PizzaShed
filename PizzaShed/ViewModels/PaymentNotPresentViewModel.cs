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
    public class PaymentNotPresentViewModel : ViewModelBase
    {
        private ICheckoutViewModel _checkoutViewModel;

        public string Total => _checkoutViewModel.TotalPriceValue;

        public decimal TotalValue => Convert.ToDecimal(Total.Replace("£", ""));

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        private string? _cardNo;
        public string? CardNo
        {
            get => _cardNo;
            set
            {

                if (value != null && value.Length == 16 && value.All(char.IsDigit))
                {
                    ErrorMessage = null;
                    SetProperty(ref _cardNo, value);
                } 
                else
                {
                    SetProperty(ref _cardNo, null);
                    ErrorMessage = "Invalid Card Number";
                }
            }
        }

        private string? _expMonth;
        public string? ExpMonth
        {
            get => _expMonth;
            set
            {
                if (value != null && value.Length == 2)
                {
                    // We can just do a manual check for valid months
                    if (value[0] == '1')
                    {
                        if (value[1] == '0' || value[1] == '1' || value[1] == '2')
                        {
                            ErrorMessage = null;
                            SetProperty(ref _expMonth, value);
                        }
                        else
                        {
                            SetProperty(ref _expMonth, null);
                            ErrorMessage = "Invalid Expiry Month";
                        }
                    } 
                    else if (value[0] == '0' && value[1] != '0' && char.IsDigit(value[1]))
                    {
                        ErrorMessage = null;
                        SetProperty(ref _expMonth, value);
                    }
                    else
                    {
                        SetProperty(ref _expMonth, null);
                        ErrorMessage = "Invalid Expiry Month";
                    }
                }
                else
                {
                    SetProperty(ref _expMonth, null);
                    ErrorMessage = "Expiry Month should be 2 digits";
                }
            }
        }
        
        private string? _expYear;
        public string? ExpYear
        {
            get => _expYear;
            set
            {
                int currentYear = DateTime.Now.Year;                                

                if (value != null && value.Length == 4 && int.TryParse(value, out int inputYear))
                {
                    if (inputYear >= currentYear && inputYear < currentYear + 6)
                    {
                        if (inputYear == currentYear && _expMonth != "")
                        {
                            int currentMonth = DateTime.Now.Month;
                            if (int.TryParse(_expMonth, out int inputMonth))
                            {
                                if (inputMonth > currentMonth)
                                {
                                    ErrorMessage = null;
                                    SetProperty(ref _expYear, value);
                                }
                                else
                                {
                                    SetProperty(ref _expYear, null);                                    
                                    ErrorMessage = "Card Expired";
                                }
                            }
                            else
                            {
                                SetProperty(ref _expYear, null);
                                ErrorMessage = "Please enter the expiration month";
                            }                                    
                        }                        
                        else
                        {
                            ErrorMessage = null;
                            SetProperty(ref _expYear, value);
                        }
                    }
                    else
                    {
                        SetProperty(ref _expYear, null);
                        ErrorMessage = "Invalid Expiry Year";
                    }
                }
                else
                {
                    SetProperty(ref _expYear, null);
                    ErrorMessage = "Invalid Expiry Year";
                }
            }
        }

        private string? _ccv;
        public string? CCV
        {
            get => _ccv;
            set
            {
                if (value != null && value.Length == 3 && value.All(char.IsDigit))
                {
                    ErrorMessage = null;
                    SetProperty(ref _ccv, value);
                }
                else
                {
                    SetProperty(ref _ccv, null);   
                    ErrorMessage = "Invalid CCV";
                }
            }
        }

        public ICommand MakePaymentCommand { get; }
        public ICommand CancelPaymentCommand { get; }

        public PaymentNotPresentViewModel(ICheckoutViewModel checkoutViewModel) 
        { 
            _checkoutViewModel = checkoutViewModel;

            MakePaymentCommand = new RelayGenericCommand(CheckValidDetails);
            CancelPaymentCommand = new RelayGenericCommand(_checkoutViewModel.CancelPayment);
        }
        
        private void CheckValidDetails()
        {
            // We reset these properties in case the month
            // is changed to an expired value after the year
            ExpMonth = _expMonth;
            ExpYear = _expYear;

            if (
                CardNo == null
                || ExpMonth == null
                || ExpYear == null
                || CCV == null
                || ErrorMessage != null
            )
                return;
            
            // Only process the payment when there are no error's                        
            _checkoutViewModel.MakePayment(TotalValue);            
        }
    }
}
