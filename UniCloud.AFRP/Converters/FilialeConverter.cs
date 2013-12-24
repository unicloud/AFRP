using Microsoft.Practices.ServiceLocation;
using System;
using System.Globalization;
using System.Windows.Data;
using UniCloud.Fleet.Services;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Converters
{
    public class FilIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int subType = (int)value;
            string subTypeName = "分公司";
            switch (subType)
            {
                case 0:
                    break;
                case 1:
                    subTypeName = "子公司（自己上报计划）"; break;
                case 2:
                    subTypeName = "子公司"; break;
                default:
                    break;
            }
            return subTypeName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string subTypeName = value.ToString();
            int subType = 0;
            switch (subTypeName)
            {
                case "分公司":
                    break;
                case "子公司（自己上报计划）":
                    subType = 1; break;
                case "子公司":
                    subType = 2; break;
                default:
                    break;
            }
            return subType;
        }
    }

    public class IdStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
            Guid? masterID = (Guid?)value;
            string masterName = "";
            if (masterID != null)
            {
                masterName = _service.CurrentAirlines.Name;
            }
            return masterName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int filStatus = (int)value;
            string filStatusName = "在用";
            switch (filStatus)
            {
                case 0:
                    break;
                case 1:
                    filStatusName = "已删除"; break;
                default:
                    break;
            }
            return filStatusName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filStatusName = value.ToString();
            int filStatus = 0;
            switch (filStatusName)
            {
                case "在用":
                    break;
                case "已删除":
                    filStatus = 1; break;
                default:
                    break;
            }
            return filStatus;
        }
    }


    public class SupplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int supStatus = (int)value;
            string supStatusName = "国内供应商";
            switch (supStatus)
            {
                case 1:
                    break;
                case 2:
                    supStatusName = "国外供应商"; break;
                default:
                    break;
            }
            return supStatusName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string supStatusName = value.ToString();
            int supStatus = 1;
            switch (supStatusName)
            {
                case "国内供应商":
                    break;
                case "国外供应商":
                    supStatus = 2; break;
                default:
                    break;
            }
            return supStatus;
        }
    }

}
