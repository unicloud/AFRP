using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Converters
{

   public class RequestApprovalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ah = value as ApprovalHistory;
            if (ah == null)
            {
                return null;
            }
            else
            {
                if (ah.Request != null && ah.Request.ApprovalDocID != null)
                {
                    return "批文";
                }
                else
                {
                    return "申请";
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
