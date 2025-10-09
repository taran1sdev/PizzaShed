using PizzaShed.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.Services.Data
{
    public interface ICustomerRepository
    {
        public Customer? GetCustomerByPartialName(string partialName);

        public bool CreateNewCustomer(Customer customer);
    }
}
