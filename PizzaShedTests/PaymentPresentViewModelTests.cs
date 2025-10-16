using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using Moq;
using PizzaShed.Model;
using PizzaShed.ViewModels;

namespace PizzaShedTests
{
    [TestFixture]
    public class PaymentPresentViewModelTests
    {
        private Mock<ICheckoutViewModel> _checkoutViewModel;
        private PaymentPresentViewModel _paymentPresentViewModel;
        private const string initialPrice = "25.25";

        [SetUp]
        public void SetUp()
        {
            _checkoutViewModel = new Mock<ICheckoutViewModel>();

            _checkoutViewModel.SetupGet(c => c.TotalPriceValue).Returns("£" + initialPrice);
            
            _paymentPresentViewModel = new PaymentPresentViewModel(_checkoutViewModel.Object);

        }

        [Test]
        public void Constructor_InitializesTotal_FromCheckoutViewModel()
        {
            Assert.That(_paymentPresentViewModel.Total, Is.EqualTo("£" + initialPrice));
            Assert.That(_paymentPresentViewModel.TotalValue, Is.EqualTo(25.25m));
        }

        [Test]
        public void Constructor_InitializesCommands()
        {
            Assert.That(_paymentPresentViewModel.ButtonCommand, Is.Not.Null);  
            Assert.That(_paymentPresentViewModel.CancelPaymentCommand, Is.Not.Null);   
            Assert.That(_paymentPresentViewModel.MakePaymentCommand, Is.Not.Null);
        }

        [TestCase("10.00", "10.00")]
        [TestCase("£10.00", "10.00")]
        [TestCase("", "0.00")]
        public void Total_Setter_RemovesPoundSymbol(string input, string expectedValue)
        {
            _paymentPresentViewModel.Total = expectedValue;

            string actualValue = _paymentPresentViewModel.Total;

            Assert.That(actualValue, Is.EqualTo("£" + expectedValue));
        }

        [TestCase("123.45", 123.45)]
        [TestCase("10", 10.00)]
        [TestCase("0.5", 0.50)]
        [TestCase("", 0.00)]
        [TestCase("0", 0.00)]
        public void TotalValue_Getter_ConvertsToDecimal(string input, decimal expectedValue)
        {
            _paymentPresentViewModel.Total = input;

            Assert.That(_paymentPresentViewModel.TotalValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ButtonCommand_Clear_ClearsTotal()
        {
            _paymentPresentViewModel.Total = "12.00";

            _paymentPresentViewModel.ButtonCommand.Execute("Clear");

            Assert.That(_paymentPresentViewModel.Total, Is.EqualTo("£"));
        }

        [TestCase("10", "£10.")]
        [TestCase("10.0", "£10.0")]
        [TestCase("", "£0.")]
        public void ButtonCommand_Point_OnlyAddsPointWhenAllowed(string input, string expectedValue)
        {
            _paymentPresentViewModel.Total = input;

            _paymentPresentViewModel.ButtonCommand.Execute("Point");

            Assert.That(_paymentPresentViewModel.Total, Is.EqualTo(expectedValue));
        }

        [TestCase("1", "£10")]
        [TestCase("10.", "£10.0")]
        [TestCase("10.5", "£10.50")]
        [TestCase("10.50", "£10.50")]
        public void ButtonCommand_Zero_AppendsCorrectly(string input, string expectedValue)
        {
            _paymentPresentViewModel.Total = input;

            _paymentPresentViewModel.ButtonCommand.Execute("0");

            Assert.That(_paymentPresentViewModel.Total, Is.EqualTo(expectedValue));
        }

        [TestCase("1", "9", "£19")]
        [TestCase("10", "8", "£108")]
        [TestCase("10.5", "6", "£10.56")]
        [TestCase("10.55", "4", "£10.55")]
        [TestCase("", "9", "£9")]
        public void ButtonCommand_Digit_AppendsCorrectly(string input, string digit, string expectedValue)
        {
            _paymentPresentViewModel.Total = input;

            _paymentPresentViewModel.ButtonCommand.Execute(digit);

            Assert.That(_paymentPresentViewModel.Total, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CancelPayment_ButtonCommoand_Calls_CancelPayment()
        {
            _paymentPresentViewModel.CancelPaymentCommand.Execute(null);

            _checkoutViewModel.Verify(c => c.CancelPayment(), Times.Once);
        }

        [Test]
        public void MakePayment_ButtonCommand_Calls_MakePayment_WhenTotalNotZero()
        {
            _paymentPresentViewModel.MakePaymentCommand.Execute(null);

            _checkoutViewModel.Verify(c => c.MakePayment(_paymentPresentViewModel.TotalValue), Times.Once);
        }

        [Test]
        public void MakePayment_ButtonCommand_DoesNotCall_MakePayment_WhenTotalZero()
        {
            _paymentPresentViewModel.Total = "";

            _paymentPresentViewModel.MakePaymentCommand.Execute(null);

            _checkoutViewModel.Verify(c => c.MakePayment(It.IsAny<decimal>()), Times.Never);
        }
    }
}
