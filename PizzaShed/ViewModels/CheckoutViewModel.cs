using PizzaShed.Services.Data;
using PizzaShed.Model;
using PizzaShed.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PizzaShed.Services.Logging;

namespace PizzaShed.ViewModels
{
    public class CheckoutViewModel : ViewModelBase
    {
        private ISession _session;
        private IOrderRepository _orderRepository;
        private readonly Order? _currentOrder;
        
        public ObservableCollection<Product> CurrentOrder => _currentOrder?.OrderProducts ?? [];
        
        public ObservableCollection<Product> OrderProducts 
        {
            get
            {
                if (_currentOrder != null)
                {

                    return _currentOrder.OrderProducts;
                }
                return [];
            }
        }
        //------    View    ------//
        public bool IsCollection => _currentOrder?.OrderType == "Collection";

        private ObservableCollection<Promotion> _promotions = [];
        public ObservableCollection<Promotion> Promotions
        {
            get => _promotions;
            set
            {
                SetProperty(ref _promotions, value);
            }
        }

        private Promotion? _selectedPromotion = null;
        public Promotion? SelectedPromotion 
        {
            get => _selectedPromotion; 
            set
            {
                if (SetProperty(ref _selectedPromotion, value)) 
                {
                    SelectPromotion();
                    OnPropertyChanged(nameof(DiscountValue));
                    OnPropertyChanged(nameof(VATValue));
                    OnPropertyChanged(nameof(TotalPriceValue));
                }
            }
        }

        public bool EligibleForPromotion => Promotions.Count > 0;

        //------    ORDER   ------//
        public string OrderNo => $"Order #{_currentOrder?.ID:0000}";

        public string DiscountValue
        {
            get
            {
                if (SelectedPromotion != null)
                    return $"-£{_currentOrder?.TotalPrice * SelectedPromotion.DiscountValue:N2}";
                return "£0.00";
            }
        }

        public string VATValue => $"£{_currentOrder?.VAT:N2}";

        public string TotalPriceValue => $"£{_currentOrder?.TotalPrice:N2}";

        public ICommand BackCommand { get; }

        public ICommand LogoutCommand { get; }

        public CheckoutViewModel(IOrderRepository orderRepo, ISession session, int orderID)
        {
            _orderRepository = orderRepo;
            _session = session;
            
            if (orderID > 0)
            {
                _currentOrder = _orderRepository.GetOrderByOrderNumber(orderID);
                if (_currentOrder != null) 
                    Promotions = _orderRepository.FetchEligiblePromotions(_currentOrder.PriceExcludingDeals);
            }

            BackCommand = new RelayGenericCommand(OnBack);
            LogoutCommand = new RelayGenericCommand(OnLogout);
        }

        private void SelectPromotion()
        {
            if (SelectedPromotion == null || _currentOrder == null)
                return;

            _currentOrder.Promo = SelectedPromotion;
        }

        private void OnBack()
        {
            if (_currentOrder == null)
                return;
                        
                
            if (_orderRepository.DeleteOrder(_currentOrder.ID))
                OnNavigate();
        }
    
        private void OnLogout()
        {
            OnBack(); // Calling this function deletes the current order on logout
            _session.Logout();
        }
    }
}
