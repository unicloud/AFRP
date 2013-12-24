using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.Converters
{
    public class ExportAnnualPlanToSeatingCapacityConverter : IValueConverter
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
                IEnumerable<AircraftBusiness> ab = value as IEnumerable<AircraftBusiness>;
                if (ab != null)
                {
                    if (ab.OrderByDescending(o => o.StartDate).FirstOrDefault() != null)
                    {
                        return ab.OrderByDescending(o => o.StartDate).FirstOrDefault().SeatingCapacity;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ExportAnnualPlanToCarryingCapacityConverter : IValueConverter
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
                IEnumerable<AircraftBusiness> ab = value as IEnumerable<AircraftBusiness>;
                if (ab != null)
                {
                    if (ab.OrderByDescending(o => o.StartDate).FirstOrDefault() != null)
                    {
                        return ab.OrderByDescending(o => o.StartDate).FirstOrDefault().CarryingCapacity;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
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
