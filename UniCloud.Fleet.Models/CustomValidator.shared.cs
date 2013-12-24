using System;
using System.ComponentModel.DataAnnotations;

namespace UniCloud.Fleet.Models
{
    public class PlanHistoryValidator
    {
        public static ValidationResult SeatingCapacity(int value, ValidationContext context)
        {
            var planHistory = context.ObjectInstance as PlanHistory;
            if (planHistory == null) return null;
            // 传到服务端时，只会有AircraftTypeID，没有AircraftType
            if (planHistory.AircraftTypeID != Guid.Empty && planHistory.AircraftType == null) return null;
            // 座位数验证
            var seating = Math.Abs(value);
            if (planHistory.AircraftType == null || planHistory.AircraftType.AircraftCategory == null)
                return new ValidationResult("请先选择机型", new[] { "AircraftTypeID" });

            var regional = planHistory.AircraftType.AircraftCategory.Regional;
            switch (regional)
            {
                case "250座以上客机":
                    return seating > 200
                               ? ValidationResult.Success
                               : new ValidationResult("座位数应大于200", new[] { "SeatingCapacity" });
                case "100-200座客机":
                    return seating > 100 && seating < 250
                               ? ValidationResult.Success
                               : new ValidationResult("座位数应在100~250之间", new[] { "SeatingCapacity" });
                case "100座以下客机":
                    return seating < 100
                               ? ValidationResult.Success
                               : new ValidationResult("座位数应小于100", new[] { "SeatingCapacity" });
                default:
                    return ValidationResult.Success;
            }
        }

    }

}
