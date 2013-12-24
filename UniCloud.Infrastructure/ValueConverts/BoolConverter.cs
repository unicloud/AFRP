using System;
using System.Windows;
using System.Windows.Data;

namespace UniCloud.Infrastructure.ValueConverts
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region "IValueConverter Members"

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                return ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (parameter.ToString() == "Inverse")
            {
                return ((bool)value == true) ? Visibility.Collapsed : Visibility.Visible;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }



    public class WhetherConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null) { return string.Empty; }
            if (value == null) { return string.Empty; }

            string result = string.Empty;

            if ((bool)value)
            {
                switch (parameter.ToString())
                {
                    case "Valid":
                        result = "有效";
                        break;
                    case "Approve":
                        result = "批准"; 
                        break;
                    case "Submit":
                        result = "上报"; 
                        break;
                    case "Operation":
                        result = "运营"; 
                        break;
                    default:
                        result = string.Empty; 
                        break;
                }
            }
            else
            {
                switch (parameter.ToString())
                {
                    case "Valid":
                        result = "无效";
                        break;
                    case "Approve":
                        result = "未批准";
                        break;
                    case "Submit":
                        result = "未上报"; 
                        break;
                    case "Operation":
                        result = "未运营"; 
                        break;
                    default:
                        result = string.Empty; 
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

}
