using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WaveEngine_MRTK_Demo.Common.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var boolValue = value as bool?;

            //If parameter has ANY value then the bool value is inverted.
            if (parameter != null && boolValue.HasValue)
            {
                boolValue = !boolValue;
            }

            return boolValue.HasValue && !boolValue.Value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
