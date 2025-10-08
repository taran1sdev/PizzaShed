using PizzaShed.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShed.ViewModels
{
    public class CheckoutViewModel : ViewModelBase
    {
        private ISession _session;
        private IOrderRepository _orderRepository;

        public CheckoutViewModel(IOrderRepository orderRepo, ISession session)
        {
            _orderRepository = orderRepo;
            _session = session;            
        }
    }
}
