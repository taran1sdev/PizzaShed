using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PizzaShed.Helpers
{
    public class DeliveryStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                if (isSelected)
                {
                    return Application.Current.FindResource("DeliverySelected");
                }
                else
                {
                    return Application.Current.FindResource("DeliveryButtonStyle");
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default!;
        }
    }
}
