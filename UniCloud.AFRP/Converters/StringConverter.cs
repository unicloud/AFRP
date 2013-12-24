using System.Windows.Data;
using UniCloud.Fleet.Models;
using UniCloud.Infrastructure;

namespace UniCloud.AFRP.Converters
{
    /// <summary>
    /// 根据枚举的整型值获取模型批注中的列名
    /// </summary>
    public class EnumToStringValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var type = parameter.ToString();
            object result = null;
            switch (type)
            {
                case "AirlinesType":
                    result = EnumUtility.GetName(typeof(AirlinesType), (AirlinesType)value);
                    break;
                case "ManageStatus":
                    result = EnumUtility.GetName(typeof(ManageStatus), (ManageStatus)value);
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
                case "PlanPublishStatus":
                    result = EnumUtility.GetName(typeof(PlanPublishStatus), (PlanPublishStatus)value);
                    break;
                case "FilialeStatus":
                    result = EnumUtility.GetName(typeof(FilialeStatus), (FilialeStatus)value);
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

    /// <summary>
    /// 根据三态的IsOpen值获取显示值
    /// </summary>
    public class IsOpenToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = string.Empty;
            if (value == null) { return result; }
            else if ((bool)value) { result = "打开"; }
            else { result = "关闭"; }
            return result;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// 数值零显示为空
    /// </summary>
    public class ZeroToNullConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value == 0)
            {
                return null;
            }
            return value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

}
