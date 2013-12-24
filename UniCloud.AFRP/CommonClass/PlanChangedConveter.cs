using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using System.Linq;
namespace UniCloud.AFRP.CommonClass
{
    public class PlanChangedConveter : IValueConverter
    {
        #region IValueConverter Members
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var planHistory = value as PlanHistory;
            var comparePlan = _service.EntityContainer.GetEntitySet<Plan>().FirstOrDefault(p => p.IsFinished);
            if (planHistory != null && planHistory.Plan.Title.Contains("年度机队资源调整") && comparePlan != null)
            {
                var existComparePlanHistory =
                    comparePlan.PlanHistories.Any(p => p.PlanAircraftID == planHistory.PlanAircraftID);
                if (existComparePlanHistory)
                {
                    var comparePlanHistory = comparePlan.PlanHistories.First(p => p.PlanAircraftID == planHistory.PlanAircraftID);
                    return planHistory.PerformMonth != comparePlanHistory.PerformMonth ||
                           planHistory.PerformAnnualID != comparePlanHistory.PerformAnnualID;
                }
                else
                {
                    return false;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
