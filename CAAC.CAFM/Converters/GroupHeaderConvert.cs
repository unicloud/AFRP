using System;
using System.Linq;
using System.Windows.Data;
using UniCloud.Fleet.Models;
using Microsoft.Practices.ServiceLocation;
using CAAC.Fleet.Services;

namespace CAAC.CAFM.Converters
{
    public class GroupHeaderConvert : IValueConverter
    {
        #region IValueConverter Members
        private  IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            { 
                Guid planId =Guid.Parse(value.ToString());
                Plan plan = this._service.EntityContainer.GetEntitySet<PlanHistory>().Select(p => p.Plan).FirstOrDefault(p => p.PlanID ==planId);
                if (plan != null)
                {
                    return plan.Airlines.Name;
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
