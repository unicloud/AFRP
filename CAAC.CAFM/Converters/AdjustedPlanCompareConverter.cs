using CAAC.Fleet.Services;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Globalization;
using System.Net;
using System.Linq;
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

namespace CAAC.CAFM.Converters
{
    public class AdjustedPlanCompareConverter : IValueConverter
    {
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ah = value as PlanHistory;
            var CurrentAdjustedPlan = ah.Plan;
            var OriginalPublishedPlan = _service.EntityContainer.GetEntitySet<Plan>().Where(p => p.Airlines == CurrentAdjustedPlan.Airlines && p.AnnualID == CurrentAdjustedPlan.AnnualID && p.VersionNumber == CurrentAdjustedPlan.VersionNumber - 1).FirstOrDefault();
            var isAnOldPlanHistory = OriginalPublishedPlan.PlanHistories.Any(p => p.PlanAircraft == ah.PlanAircraft);
            var isUnChangedPlanHistory=OriginalPublishedPlan.PlanHistories.Any(p=>p.PlanAircraft==ah.PlanAircraft && p.PerformMonth==ah.PerformMonth && p.PerformAnnualID==ah.PerformAnnualID);
            var iPlanHistory = OriginalPublishedPlan.PlanHistories.Where(p => p.PlanAircraft == ah.PlanAircraft);

            if (isAnOldPlanHistory)
            {
                if (isUnChangedPlanHistory)
                {
                    return null;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
