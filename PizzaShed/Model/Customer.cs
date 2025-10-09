using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Customer
    {
        private int _id = 0;
        public int ID 
        { 
            get => _id;
            set
            {
                // this value should only be set once
                if (_id == 0)
                {
                    _id = value;
                }
            
            } 
        }

        private string _name = "";

        public string Name 
        { 
            get => _name; 
            set
            {
                // We should only be able to set this once
                if (_name == "")
                {
                    _name = value;
                }
            } 
        }

        public required string PhoneNumber { get; set; }

        public required string PostCode { get; set;  }

        public string? FlatNo { get; set; }

        public required int HouseNo { get; set; }

        public required string StreetAddress { get; set; }

        public string? DeliveryNotes { get; set; }

        // We display customers name and postcode in our listview 
        // that way if we have duplicate names in the DB the cashier
        // can check with the customer that the postcode is correct
        // before populating the form
        public string ListName => $"{Name} : {PostCode}"; 
    }
}
