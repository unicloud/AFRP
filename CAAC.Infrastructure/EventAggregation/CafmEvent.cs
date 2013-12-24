using System;
using Microsoft.Practices.Prism.Events;

namespace CAAC.Infrastructure
{
    public class OprationMessageEvent : CompositePresentationEvent<Tuple<OperationMessageType, string>> { }
    public class SetRefreshEvent : CompositePresentationEvent<bool> { }
    public class SetRefreshAnnualPlanEvent : CompositePresentationEvent<bool> { }
    public class SetRefreshlPlanEvent : CompositePresentationEvent<bool> { }

    public class IsLoadingEvent<T> : CompositePresentationEvent<bool> { }


    public enum OperationMessageType { Success, Fail, Info }


}