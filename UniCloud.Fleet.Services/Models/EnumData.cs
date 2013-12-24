namespace UniCloud.Fleet.Models
{
    public enum PlanRelativeSubmitState
    {
        All = 0,
        OperationHistoryNot = 1,
        BusinessHistoryNot = 2,
        RequestNot = 3
    }

    public enum PlanDetailCreateSource
    {
        New = 0,
        PlanAircraft = 1,
        Aircraft = 2
    }

}
