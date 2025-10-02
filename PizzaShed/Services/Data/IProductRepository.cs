using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using PizzaShed.Model;

namespace PizzaShed.Services.Data
{
    public interface IProductRepository<T> where T : MenuItemBase
    {
        public List<T> GetProductsByCategory(string category, string? size);

        public List<T> GetProductsByCategory(string category);

        public List<T> GetMealDeals();
    }
}
