using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.Converters
{
    public class UnSelectedRequestToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Colors.Yellow.ToString();
            else
                return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class ApplyStyle : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is PlanHistory)
            {
                PlanHistory planhistory = item as PlanHistory;
                if (planhistory.IsApply=="未申请")
                {
                    return NotApplyStyle;
                }
                else
                {
                    return IsApplyStyle;
                }
            }

            return null;
        }
        public Style IsApplyStyle { get; set; }
        public Style NotApplyStyle { get; set; }
    }

}