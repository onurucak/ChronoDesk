using System;
using System.Globalization;
using System.Windows.Data;
using ChronoDesk.UI.ViewModels;

namespace ChronoDesk.UI.Converters;

public class ViewModelTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ViewModelBase vm && parameter is Type type)
        {
            return vm.GetType() == type;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
