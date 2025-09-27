using System;
using NUnit.Framework;
using PizzaShed.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace PizzaShedTests
{
    [TestFixture]
    public class PasswordBoxHelperTests
    {
        private PasswordBox _passwordBox;

        [SetUp]
        [Apartment(ApartmentState.STA)] // We must run run in a Single Threaded Apartment state to test UI elements
        public void SetUp()
        {
            _passwordBox = new PasswordBox();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        // Check that GetBoundPassword gets the correct value from BoundPassword
        public void GetBoundPassword_Retrieves_Value_From_PasswordBox()
        {
            const string expectedPin = "123";

            // Set the bound password property
            _passwordBox.SetValue(PasswordBoxHelper.BoundPasswordProperty, expectedPin);

            string pin = PasswordBoxHelper.GetBoundPassword(_passwordBox);

            // Check the value matches
            Assert.That(pin, Is.EqualTo(expectedPin), "GetBoundPassword should retrieve the value stored in BoundPassword");
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        // Check that SetBoundPassword sets BoundPassword value correctly
        public void SetBoundPassword_Sets_PasswordBox_BoundPassword()
        {
            const string expectedPin = "123";

            // Set the value with SetBoundPassword function
            PasswordBoxHelper.SetBoundPassword(_passwordBox, expectedPin);

            // Get the actual value stored in BoundPassword
            string pin = (string)_passwordBox.GetValue(PasswordBoxHelper.BoundPasswordProperty);

            Assert.That(pin, Is.EqualTo(expectedPin), "SetBoundPassword should store the expected value in the BoundPassword property");
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        // Make sure SetBoundPassword updates the Password field of password box
        public void SetBoundPassword_Updates_PasswordBox_Password()
        {
            const string expectedPin = "123";

            PasswordBoxHelper.SetBoundPassword(_passwordBox, expectedPin);

            // Check PasswordBox.Password matches expected pin
            Assert.That(_passwordBox.Password, Is.EqualTo(expectedPin), "PasswordBox.Password should match the set value");            
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        // Make sure SetBoundPassword does not update the Password if values match
        public void SetBoundPassword_Does_Not_Update_If_Values_Match()
        {
            _passwordBox.Password = "123";

            // Call it twice to simulate recieving the same input multiple times
            PasswordBoxHelper.SetBoundPassword(_passwordBox, "123");
            PasswordBoxHelper.SetBoundPassword(_passwordBox, "123");

            Assert.That(_passwordBox.Password, Is.EqualTo("123"), "Password should not be updated if values match");
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        // Make sure PasswordChanged event updates Bound Password
        public void PasswordChangedEvent_Updates_Bound_Password()
        {
            const string expectedPin = "123";

            // Bound password initially empty
            PasswordBoxHelper.SetBoundPassword(_passwordBox, string.Empty);

            // Update the password box
            _passwordBox.Password = expectedPin;
            _passwordBox.PasswordChanged += PasswordBoxHelper.HandlePasswordChanged;
            
            // Trigger the event
            _passwordBox.RaiseEvent(new RoutedEventArgs(PasswordBox.PasswordChangedEvent, _passwordBox));            

            // Get the value stored in BoundPassword
            string pin = PasswordBoxHelper.GetBoundPassword(_passwordBox);

            // Check that BoundPassword was updated
            Assert.That(pin, Is.EqualTo(expectedPin), "BoundPassword should be updated when PasswordChangedEvent fires");
        }
    }
}
