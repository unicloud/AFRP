using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    public static class EventAggregatorExtensions
    {
        public static void PublishAsGeneric(this IEventAggregator aggregator, IEvent e)
        {
            Requires.NotNull(aggregator, "aggregator");
            Requires.NotNull(e, "e");

            // We need to call the generic Publish<TEvent> but we can't statically because 
            // we don't what TEvent is upfront. Use Reflection to call it, satisfying TEvent
            // with the type of the event.
            MethodInfo method = typeof(IEventAggregator).GetMethod("Publish");  // Get definition Publish<TEvent>
            method = method.MakeGenericMethod(e.GetType());                // Get instantiated Publish<MyEvent>

            method.Invoke(aggregator, new object[] { e });
        }
    }
}
