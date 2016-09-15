using System;
using System.Globalization;
using Xamarin.Forms;

namespace TaskList.Converters
{
    public class ItemVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as ItemVisibilityEventArgs;
            return eventArgs.Item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
