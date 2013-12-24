using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Data;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.Converters
{
    public class UnSelectedRequestToReadOnly : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ReqStateToEnable : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool editable;
            string[] editBehavior = parameter.ToString().Split(':');
            List<ReqStatus> states = new List<ReqStatus>();

            #region 计算

            switch (editBehavior[0])
            {
                case "less":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "greater":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "equal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "notequal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        default:
                            break;
                    }
                    break;
                case "between":

                    break;
                default:
                    break;
            }

            #endregion

            editable = states.Contains((ReqStatus)value);
            return editable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    public class ReqStateToReadOnly : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool editable;
            string[] editBehavior = parameter.ToString().Split(':');
            List<ReqStatus> states = new List<ReqStatus>();

            #region 计算

            switch (editBehavior[0])
            {
                case "less":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "greater":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "equal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "notequal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        default:
                            break;
                    }
                    break;
                case "between":

                    break;
                default:
                    break;
            }

            #endregion

            editable = !states.Contains((ReqStatus)value);
            return editable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ReqStateToVisibility : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] editBehavior = parameter.ToString().Split(':');
            List<ReqStatus> states = new List<ReqStatus>();

            #region 计算

            switch (editBehavior[0])
            {
                case "less":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "greater":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "equal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Draft);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Checking);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Checked);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Submited);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Examined);
                            break;
                        default:
                            break;
                    }
                    break;
                case "notequal":
                    switch (editBehavior[1].ToLower())
                    {
                        case "draft":
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checking":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "checked":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Submited);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "submited":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Examined);
                            break;
                        case "examined":
                            states.Add(ReqStatus.Draft);
                            states.Add(ReqStatus.Checking);
                            states.Add(ReqStatus.Checked);
                            states.Add(ReqStatus.Submited);
                            break;
                        default:
                            break;
                    }
                    break;
                case "between":

                    break;
                default:
                    break;
            }

            #endregion

            return states.Contains((ReqStatus)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}