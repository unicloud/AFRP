﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.Converters
{
    public class PlanHistoryRequset : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ApprovalHistory ah = value as ApprovalHistory;
            if (value == null)
            {
                return null;
            }
            else
            {
                return "申请";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PlanHistoryDefinite : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            if (value == null)
            {
                return null;
            }
            else
            {

                if (value.GetType() == typeof(OperationPlan))
                {
                    var ph = value as OperationPlan;
                    if ((ph as OperationPlan).OperationHistory != null)
                    {
                        return "运营历史";
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var ph = value as ChangePlan;
                    if ((ph as ChangePlan).AircraftBusiness != null)
                    {
                        return "商业数据";
                    }
                    else
                    {
                        return null;
                    }


                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
