using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace PizzaShed.Helpers
{
    public class CategoryStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string buttonCategory = value?.ToString() ?? string.Empty;

            string selectedCategory = parameter?.ToString() ?? string.Empty;

            if (selectedCategory.Equals(buttonCategory, StringComparison.OrdinalIgnoreCase))
            {
                return Application.Current.FindResource("CategorySelected");
            }

            return Application.Current.FindResource("CategoryNotSelected");
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default!;
        }
    }
}
