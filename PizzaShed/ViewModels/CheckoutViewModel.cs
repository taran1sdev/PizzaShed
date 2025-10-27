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
using System.Data;
using System.Windows;

namespace PizzaShed.ViewModels
{
    public class CheckoutViewModel : ViewModelBase, ICheckoutViewModel
    {
        private ISession _session;
        private IOrderRepository _orderRepository;
        private readonly Order _currentOrder;
        
        public ObservableCollection<Product> CurrentOrder => _currentOrder.OrderProducts;
        
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
        public bool IsCollection => _currentOrder.OrderType == "Collection";
        
        public bool IsDelivery => !IsCollection;

        private bool _isPhone;
        public bool IsPhone
        {
            get => _isPhone;
            set => SetProperty(ref _isPhone, value);
        }

        public ICommand SelectPhoneCommand { get; }

        private string _expectedDeliveryTime = "";
        public string ExpectedDeliveryTime
        {
            get => _expectedDeliveryTime;
            set => SetProperty(ref _expectedDeliveryTime, value);
        }

        // Holds our collection times
        private ObservableCollection<string> _collectionTimes = [];
        public ObservableCollection<string> CollectionTimes
        {
            get => _collectionTimes;
            set => SetProperty(ref _collectionTimes, value);

        }

        private string? _selectedCollectionTime;
        public string? SelectedCollectionTime
        {
            get => _selectedCollectionTime;
            set
            {
                SetProperty(ref _selectedCollectionTime, value);
                SelectCollectionTime();
            }
        }

        // Toggles the payment button visibility 
        private bool _acceptOrder;
        public bool AcceptOrder
        {
            get => _acceptOrder && !IsPaid;
            set => SetProperty(ref _acceptOrder, value);
        }

        // We use this property for the cash button as the driver needs
        // to make cash payments on return
        private bool _isDriverCash;
        public bool IsDriverCash
        {
            get => AcceptOrder && !IsPaid || _session.UserRole == "Driver" && !IsPaid;
            set => SetProperty(ref _isDriverCash, value);
        }

        // We hide the order notes for orders that have been completed
        public bool NotesVisible => _currentOrder.OrderStatus == "New"; 

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
        public string OrderNo => $"Order #{_currentOrder.ID:0000}";

        public int OrderID => _currentOrder.ID;

        public string DeliveryValue
        {
            get
            {
                if (IsDelivery && _currentOrder?.DeliveryFee != null)
                    return $"£{_currentOrder.DeliveryFee:N2}";
                return "";
            }
        }

        public string DiscountValue
        {
            get
            {
                if (SelectedPromotion != null)
                {
                    decimal total = _currentOrder.TotalPrice;
                    if (_currentOrder.DeliveryFee != null)
                        total -= (decimal)_currentOrder.DeliveryFee; // We do not apply discounts to the set delivery fee
                    return $"-£{total * SelectedPromotion.DiscountValue:N2}";
                }
                    
                return "£0.00";
            }
        }

        public string VATValue => $"£{_currentOrder.VAT:N2}";

        public string TotalPriceValue 
            => $"£{_currentOrder.PriceAfterPayments:N2}";

        private bool CardPayment = false;

        private string PaymentType => CardPayment ? "Card" : "Cash";

        public bool IsPaid 
        {
            get => _currentOrder.PriceAfterPayments <= (decimal)0.00;
        }

        private string _orderSource = "";
        public string OrderSource
        {
            get => _orderSource;
            set => SetProperty(ref _orderSource, value);
        }

        public string? OrderNotes
        {
            get => _currentOrder.OrderNotes;
            set => _currentOrder.OrderNotes = value;
        }

        public ICommand BackCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CardCommand { get;  }
        public ICommand CashCommand { get; }

        public ICommand CompleteOrderCommand { get; }

        public CheckoutViewModel(IOrderRepository orderRepo, ISession session, int orderID)
        {
            _orderRepository = orderRepo;
            _session = session;            

            _currentOrder = _orderRepository.GetOrderByOrderNumber(orderID) 
                ?? new Order{
                OrderStatus = "Error"
                }; 
                
            if (_currentOrder.OrderStatus == "Error")
            {                    
                OnNavigateBack();
            }                 
              
            Promotions = _orderRepository.FetchEligiblePromotions(_currentOrder.PriceExcludingDeals);

            CompleteOrderCommand = new RelayGenericCommand(UpdateOrder);

            if (IsCollection && _currentOrder.OrderStatus == "New")
            {
                OrderSource = "Counter";
                (AcceptOrder, CollectionTimes) = _orderRepository.GetCollectionTimes();
                SelectedCollectionTime = CollectionTimes.FirstOrDefault();
            }
            else if (_currentOrder.OrderStatus == "Out For Delivery")
            {
                AcceptOrder = false;
                ExpectedDeliveryTime = "Delivered";
                Promotions = [];
                CompleteOrderCommand = new RelayGenericCommand(CompleteOrder);
            }
            else if (_currentOrder.OrderStatus == "Order Ready")
            {
                AcceptOrder = true;
                Promotions = [];
                CollectionTimes = [];
                CompleteOrderCommand = new RelayGenericCommand(CompleteOrder);
            }
            else 
            {
                OrderSource = "Phone";
                (AcceptOrder, ExpectedDeliveryTime) = _orderRepository.GetDeliveryTime();
            }

            _currentOrder.Payments.Add("Cash", []);
            _currentOrder.Payments.Add("Card", []);

            SelectPhoneCommand = new RelayGenericCommand(SelectPhone);
            BackCommand = new RelayGenericCommand(OnBack);
            LogoutCommand = new RelayGenericCommand(OnLogout);
            CardCommand = new RelayGenericCommand(OnCard);
            CashCommand = new RelayGenericCommand(OnCash);
            
        }

        private void SelectPromotion()
        {
            if (SelectedPromotion == null || _currentOrder == null)
                return;

            _currentOrder.Promo = SelectedPromotion;
        }

        private void SelectCollectionTime()
        {
            if (!AcceptOrder || SelectedCollectionTime == null)
                return;

            _currentOrder.CollectionTime = Convert.ToDateTime(SelectedCollectionTime);
        }

        private void SelectPhone()
        {
            IsPhone = !IsPhone;     
            if (IsPhone)
            {
                OrderSource = "Phone";
            }
            else
            {
                OrderSource = "Counter";
            }
        }

        private void OnBack()
        {                                        
            if (_orderRepository.DeleteOrder(_currentOrder.ID))
                OnNavigateBack();
        }
    
        private void OnLogout()
        {            
            OnBack(); // Calling this function deletes the current order on logout
            _session.Logout();
        }

        private void OnCard()
        {
            CardPayment = true;
            OnNavigate();
        }

        private void OnCash()
        {
            CardPayment = false;

            if (IsDelivery || IsPhone)
            {
                // We want to display the Payment View for the driver returning
                if (_session.UserRole == "Driver")
                {
                    OnNavigate();
                    return;
                }

                // We add this to the payments just to trigger our navigation logic
                // But we do not update the database until an actual payment is made                                    
                _currentOrder.Payments["Cash"].Add(_currentOrder.TotalPrice);


                OnPropertyChanged(nameof(TotalPriceValue));
                OnPropertyChanged(nameof(AcceptOrder));
                OnPropertyChanged(nameof(IsPaid));                
            }

            OnNavigate();
        }

        public void MakePayment(decimal amount)
        {
            // Make sure the payment has been recorded successfully in the database before doing anything else
            if (_orderRepository.CreatePayment(_currentOrder.ID, amount, PaymentType))
            {
                _currentOrder.Payments[PaymentType].Add(amount);

                OnPropertyChanged(nameof(TotalPriceValue));
                OnPropertyChanged(nameof(AcceptOrder));
                OnPropertyChanged(nameof(IsPaid));

                OnNavigate();                
            }

            
        }

        public void CancelPayment() => OnNavigate();

        // We can use this for completing the order and cancelling a current payment method
        private void UpdateOrder()
        {
            _currentOrder.OrderSource = OrderSource;
            if (_orderRepository.UpdatePaidOrder(_currentOrder))
                OnNavigate();
        }

        private void CompleteOrder()
        {
            _orderRepository.CompleteOrder(_currentOrder.ID);
            OnNavigate();
        }
    }
}
