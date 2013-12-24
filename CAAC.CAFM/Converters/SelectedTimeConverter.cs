using System;
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
using CAAC.Fleet.Services;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Linq;

namespace CAAC.CAFM.Converters
{
    public static class DateTimeConverter
    {
        public static DateTime SelectedTime = Convert.ToDateTime("2012-03-31");
    }
    public class SelectedTimeConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            if (parameter.ToString() == "SeatingCapacity")//座位
            {
                IEnumerable<AircraftBusiness> IEnumerable = value as IEnumerable<AircraftBusiness>;
                AircraftBusiness aircraftbusiness = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (aircraftbusiness != null)
                {
                    return aircraftbusiness.SeatingCapacity;
                }
            }
            else if (parameter.ToString() == "CarryingCapacity")//商载
            {
                IEnumerable<AircraftBusiness> IEnumerable = value as IEnumerable<AircraftBusiness>;
                AircraftBusiness aircraftbusiness = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (aircraftbusiness != null)
                {
                    return aircraftbusiness.CarryingCapacity;
                }
            }
            else if (parameter.ToString() == "ImportCategory.ActionName")//引进方式
            {
                IEnumerable<OperationHistory> IEnumerable = value as IEnumerable<OperationHistory>;
                OperationHistory operationhistory = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (operationhistory != null)
                {
                    return operationhistory.ImportCategory.ActionName;
                }
            }
            else if (parameter.ToString() == "Airlines.Name")//运营权
            {
                IEnumerable<OperationHistory> IEnumerable = value as IEnumerable<OperationHistory>;
                OperationHistory operationhistory = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (operationhistory != null)
                {
                    return operationhistory.Airlines.Name;
                }
            }
            else if (parameter.ToString() == "Owner.Name")//所有权
            {
                IEnumerable<OwnershipHistory> IEnumerable = value as IEnumerable<OwnershipHistory>;
                OwnershipHistory ownershiphistory = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (ownershiphistory != null)
                {
                    return ownershiphistory.Owner.Name;
                }
            }
            else if (parameter.ToString() == "AircraftType.AircraftCategory.Regional")//座级
            {
                IEnumerable<AircraftBusiness> IEnumerable = value as IEnumerable<AircraftBusiness>;
                AircraftBusiness aircraftbusiness = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (aircraftbusiness != null)
                {
                    return aircraftbusiness.AircraftType.AircraftCategory.Regional;
                }
            }
            else if (parameter.ToString() == "AircraftType.Name")//机型
            {
                IEnumerable<AircraftBusiness> IEnumerable = value as IEnumerable<AircraftBusiness>;
                AircraftBusiness aircraftbusiness = IEnumerable.FirstOrDefault(p => p.StartDate <= DateTimeConverter.SelectedTime && !(p.EndDate != null && p.EndDate < DateTimeConverter.SelectedTime));
                if (aircraftbusiness != null)
                {
                    return aircraftbusiness.AircraftType.Name;
                }
            }
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
