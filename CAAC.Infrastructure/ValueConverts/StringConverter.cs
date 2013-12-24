using System.Windows.Data;

namespace CAAC.Infrastructure.ValueConverts
{
    public class EnumToStringValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return EnumUtility.GetName(value.GetType(), value);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //return EnumUtility.GetValue(targetType, value.ToString());
            return null;
        }

        #endregion
    }
}
