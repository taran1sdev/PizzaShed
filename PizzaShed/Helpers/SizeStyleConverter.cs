using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;


namespace PizzaShed.Helpers
{
    public class SizeStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string buttonCategory = value?.ToString() ?? string.Empty;

            string selectedCategory = parameter?.ToString() ?? string.Empty;    

            if (selectedCategory.Equals(buttonCategory, StringComparison.OrdinalIgnoreCase))
            {
                return Application.Current.FindResource("SizeSelected");
            }

            return Application.Current.FindResource("SizeNotSelected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default!;
        }
    }
}
