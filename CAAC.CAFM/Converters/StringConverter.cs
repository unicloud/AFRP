using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using UniCloud.Fleet.Models;
using CAAC.Infrastructure;
using Microsoft.Practices.Prism.Events;

namespace CAAC.CAFM.Converters
{
    public class EnumIntToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string type = parameter.ToString();
            object result = null;
            switch (type)
            {
                case "AirlinesType":
                    result = EnumUtility.GetName(typeof(AirlinesType), (AirlinesType)value);
                    break;
                case "AircraftCategory":
                    result = EnumUtility.GetName(typeof(AircraftCategory), (AircraftCategory)value);
                    break;
                case "AgreementType":
                    result = EnumUtility.GetName(typeof(AgreementType), (AgreementType)value);
                    break;
                case "AgreementPhase":
                    result = EnumUtility.GetName(typeof(AgreementPhase), (AgreementPhase)value);
                    break;
                case "OpStatus":
                    result = EnumUtility.GetName(typeof(OpStatus), (OpStatus)value);
                    break;
                case "ReqStatus":
                    result = EnumUtility.GetName(typeof(ReqStatus), (ReqStatus)value);
                    break;
                case "PlanStatus":
                    result = EnumUtility.GetName(typeof(PlanStatus), (PlanStatus)value);
                    break;
                default:
                    break;
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
