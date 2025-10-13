using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Customer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newvalue, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newvalue))
            {
                return false;
            }

            field = newvalue;

            OnPropertyChanged(name);

            return true;
        }

        public int ID { get; set; } = 0;

        public string _name = "";
        public string Name 
        {
            get => _name;
            set => SetProperty(ref _name, value); 
        }

        private string _numberError = "";
        public string NumberError
        {
            get => _numberError;
            private set => SetProperty(ref _numberError, value);
        }

        // Regex to ensure a valid number is input
        private readonly Regex numberRegex = new Regex(@"^\(?0( *\d\)?){10}$");

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (numberRegex.IsMatch(value))
                {
                    SetProperty(ref _phoneNumber, value);
                    NumberError = "";
                }
                else
                {
                    NumberError = "Invalid phone number";
                }
            }
        }

        private string _postcodeError = "";

        public string PostcodeError
        {
            get => _postcodeError;
            private set => SetProperty(ref _postcodeError, value);
        }

        // Regex for checking valid postcode
        private readonly Regex postcodeValidRegex = new Regex(@"^\(?( *\p{Lu}\)?){2}( *\d\)?){2,3}( *\p{Lu}\)?){2}$");
        // Regex for checking postcode in area
        private readonly Regex postcodeRangeRegex = new Regex(@"^\(?TA6( *\d\)?){1}( *\p{Lu}\)?){2}$");


        public string _postcode = "";
        public string Postcode 
        { 
            get => _postcode; 
            set
            {
                if (postcodeValidRegex.IsMatch(value))
                {
                    if (postcodeRangeRegex.IsMatch(value))
                    {
                        SetProperty(ref _postcode, value);
                        PostcodeError = "";
                    }
                    else
                    {
                        PostcodeError = "Too far for delivery";
                    }
                }
                else
                {
                    PostcodeError = "Invalid Postcode";
                }
            } 
        }

        private string? _flat;
        public string? Flat 
        {
            get => _flat;
            set => SetProperty(ref _flat, value);
        }

        private int _houseNo;

        public int HouseNo => _houseNo;

        public string House
        {
            get => _houseNo == 0 ? "" : _houseNo.ToString();
            set
            {
                if (int.TryParse(value, out int houseNo)) 
                    SetProperty(ref _houseNo, houseNo);                
            }
        }

        public string HouseTicket
        {
            get
            {
                if (Flat == null)
                {
                    return House;
                } 
                else
                {
                    return $"{Flat} {House}";
                }
            }
        }

        private string _streetAddress = "";
        public string StreetAddress 
        {
            get => _streetAddress;
            set => SetProperty(ref _streetAddress, value); 
        }

        private string? _deliveryNotes;
        public string? DeliveryNotes 
        {
            get => _deliveryNotes;
            set => SetProperty(ref _deliveryNotes, value);
        }

        // We display customers name and postcode in our listview 
        // that way if we have duplicate names in the DB the cashier
        // can check with the customer that the postcode is correct
        // before populating the form
        public string ListName => $"{Name} : {Postcode}"; 
    }
}
