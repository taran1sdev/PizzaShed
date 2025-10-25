using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaShed.ViewModels;

namespace PizzaShedTests.Unit
{
    public class PaymentNotPresentViewModelTests
    {
        private Mock<ICheckoutViewModel> _checkoutViewModel;
        private PaymentNotPresentViewModel _paymentNotPresentViewModel;
        private const string initialPrice = "£12.25";

        [SetUp]
        public void SetUp()
        {
            _checkoutViewModel = new Mock<ICheckoutViewModel>();

            _checkoutViewModel.SetupGet(c => c.TotalPriceValue).Returns(initialPrice);

            _paymentNotPresentViewModel = new PaymentNotPresentViewModel(_checkoutViewModel.Object);
        }

        [Test]
        public void Constructor_InitializesTotalValue()
        {
            Assert.That(_paymentNotPresentViewModel.Total, Is.EqualTo(initialPrice));

            Assert.That(_paymentNotPresentViewModel.TotalValue, Is.EqualTo(12.25m));
        }

        [Test]
        public void Constructor_InitializesCommands()
        {
            Assert.That(_paymentNotPresentViewModel.MakePaymentCommand, Is.Not.Null);

            Assert.That(_paymentNotPresentViewModel.CancelPaymentCommand, Is.Not.Null);
        }

        [TestCase("0123012301230123", true, null)] // Valid
        [TestCase("012301230123012", false, "Invalid Card Number")] // Too short
        [TestCase("01230123012301230", false, "Invalid Card Number")] // Too long
        [TestCase(null, false, "Invalid Card Number")] // null value
        public void CardNoSetter_Validates_Input(string? input, bool isValid, string? error)
        {
            _paymentNotPresentViewModel.CardNo = input;

            Assert.That(_paymentNotPresentViewModel.CardNo, isValid ? Is.EqualTo(input) : Is.Null);
            Assert.That(_paymentNotPresentViewModel.ErrorMessage, Is.EqualTo(error));
        }

        [TestCase("01", true, null)] // Valid inputs
        [TestCase("05", true, null)]
        [TestCase("10", true, null)] 
        [TestCase("12", true, null)]
        [TestCase("13", false, "Invalid Expiry Month")] // Invalid Inputs
        [TestCase("50", false, "Invalid Expiry Month")]
        [TestCase("00", false, "Invalid Expiry Month")]
        [TestCase("1A", false, "Invalid Expiry Month")]
        [TestCase("5", false, "Expiry Month should be 2 digits")]
        public void ExpMonth_Setter_ValidatesFormat(string input, bool isValid, string? error)
        {
            _paymentNotPresentViewModel.ExpMonth = input;

            Assert.That(_paymentNotPresentViewModel.ExpMonth, isValid ? Is.EqualTo(input) : Is.Null);
            Assert.That(_paymentNotPresentViewModel.ErrorMessage, Is.EqualTo(error));
        }

        [TestCase("012", true, null)] // Valid
        [TestCase("01", false, "Invalid CCV")] // Too short
        [TestCase("0123", false, "Invalid CCV")] // Too long
        [TestCase("01A", false, "Invalid CCV")] // Contains non-digit
        [TestCase(null, false, "Invalid CCV")] // null value
        public void CCV_Setter_ValidatesFormat(string? input, bool isValid, string? error)
        {
            _paymentNotPresentViewModel.CCV = input;

            Assert.That(_paymentNotPresentViewModel.CCV, isValid ? Is.EqualTo(input) : Is.Null);
            Assert.That(_paymentNotPresentViewModel.ErrorMessage, Is.EqualTo(error));
        }

        [TestCase("10", "2026", true, null)]
        [TestCase("01", "2025", false, "Card Expired")]
        [TestCase("10", "2024", false, "Invalid Expiry Year")]
        [TestCase("10", "202", false, "Invalid Expiry Year")]
        [TestCase("10", "20242", false, "Invalid Expiry Year")]
        [TestCase("10", "2045", false, "Invalid Expiry Year")]
        [TestCase("", "2025", false, "Please enter the expiration month")]
        public void ExpYear_Setter_ValidatesExpiry(string inputMonth, string inputYear, bool isValid, string? error)
        {
            _paymentNotPresentViewModel.ExpMonth = inputMonth;
            _paymentNotPresentViewModel.ExpYear = inputYear;

            Assert.That(_paymentNotPresentViewModel.ExpYear, isValid ? Is.EqualTo(inputYear) : Is.Null);
            Assert.That(_paymentNotPresentViewModel.ErrorMessage, Is.EqualTo(error));
        }

        [Test]
        public void MakePaymentCommand_CallsMakePayment_OnValidDetails()
        {
            _paymentNotPresentViewModel.CardNo = "0123012301230123";
            _paymentNotPresentViewModel.ExpMonth = "10";
            _paymentNotPresentViewModel.ExpYear = "2026";
            _paymentNotPresentViewModel.CCV = "012";

            _paymentNotPresentViewModel.MakePaymentCommand.Execute(_paymentNotPresentViewModel.TotalValue);

            _checkoutViewModel.Verify(c => c.MakePayment(_paymentNotPresentViewModel.TotalValue), Times.Once);
        }

        [Test]
        public void MakePaymentCommnad_DoesNotCallMakePayment_OnNullDetails()
        {
            _paymentNotPresentViewModel.MakePaymentCommand.Execute(_paymentNotPresentViewModel.TotalValue);

            _checkoutViewModel.Verify(c => c.MakePayment(It.IsAny<decimal>()), Times.Never);
        }

        [TestCase("012301230123012", "10", "2026", "012")]
        [TestCase("0123012301230123", "13", "2026", "012")]
        [TestCase("0123012301230123", "10", "2020", "012")]
        [TestCase("0123012301230123", "10", "2026", "01")]
        public void MakePaymentCommand_DoesNotCallMakePayment_OnInvalidDetails(string cardInput, string monthInput, string yearInput, string ccvInput)
        {
            _paymentNotPresentViewModel.CardNo = cardInput;
            _paymentNotPresentViewModel.ExpMonth = monthInput;
            _paymentNotPresentViewModel.ExpYear = yearInput;
            _paymentNotPresentViewModel.CCV = ccvInput;

            _paymentNotPresentViewModel.MakePaymentCommand.Execute(_paymentNotPresentViewModel.TotalValue);

            _checkoutViewModel.Verify(c => c.MakePayment(It.IsAny<decimal>()), Times.Never);
        }

        [Test]
        public void CancelPaymentCommand_Calls_CancelPayment()
        {
            _paymentNotPresentViewModel.CancelPaymentCommand.Execute(null);

            _checkoutViewModel.Verify(c => c.CancelPayment(), Times.Once);
        }
    }
}
