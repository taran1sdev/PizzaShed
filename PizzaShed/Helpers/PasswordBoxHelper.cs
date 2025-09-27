using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PizzaShed.Helpers
{
    // Helper class to manage the secure password box input linking the view and the viewmodel
    public static class PasswordBoxHelper
    {
        // We define a property BoundPassword to attach to the password box 
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                // This is the function that will execute when the viewmodel value changes
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged)); 

        // We define the required getter and setter to access the value stored in the password box
        public static string GetBoundPassword(PasswordBox box)
        {
            return (string)box.GetValue(BoundPasswordProperty);
        }
        
        public static void SetBoundPassword(PasswordBox box, string value) 
        { 
            box.SetValue(BoundPasswordProperty, value);
        }

        // This is for outbound changes - if the viewmodel value changes then update the view 
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                // We remove the event handler so it isn't called again by our changes
                box.PasswordChanged -= HandlePasswordChanged;

                // Check to ensure the new value is not null and is a string
                if (e.NewValue != null && e.NewValue is string newPassword)
                {
                    // Update the Password property if the new value does not match the current value
                    if (box.Password != newPassword)
                    {
                        box.Password = newPassword;
                    }
                }

                // Add the event handler again
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        // This is for inbound changes - if the view value changes update the viewmodel
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                SetBoundPassword(box, box.Password);
            }
        }
    }
}
