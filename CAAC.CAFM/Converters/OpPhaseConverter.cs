using System;
using System.Windows.Data;
using System.Windows.Media;

namespace CAAC.CAFM.Converters
{
    public class OpPhaseConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string color;
            if (value.ToString() == "Draft")
            {
                color = Colors.Red.ToString();
            }
            else if (value.ToString() == "Checking")
            {
                color = Colors.Yellow.ToString();
            }
            else if (value.ToString() == "Checked")
            {
                color = Colors.Orange.ToString();
            }
            else
            {
                color = Colors.Green.ToString();
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
