using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Model
{
    public class Customer
    {
        public int ID { get; }

        public string Name { get; }

        public string PhoneNumber { get; }

        public string PostCode { get; }

        public string? FlatNo { get; }

        public int HouseNo { get; }

        public string StreetAddress { get; }

        public string? DeliveryNotes { get; }
    }
}
