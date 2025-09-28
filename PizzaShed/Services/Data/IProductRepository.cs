using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    internal interface IProductRepository
    {
        public List<Product> GetProductsByCategory(string category);

        public List<Product> GetProductsByCategory(string category, string size);

        public List<Product> GetToppingsByCategory(string category);
    }
}
