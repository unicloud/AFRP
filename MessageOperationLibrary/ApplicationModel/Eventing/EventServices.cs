using System;
using System.Collections.Generic;
//using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    internal class EventServices
    {
        public static EventHandler MakeWeak(EventHandler handler, Action<EventHandler> remove)
        {
            var reference = new WeakReference(handler.Target);
            var method = handler.Method;

            EventHandler newHandler = null;
            newHandler = (sender, e) =>
            {
                var target = reference.Target;
                if (target != null)
                {
                    method.Invoke(target, new object[] { sender, e });
                }
                else
                {

                    remove(newHandler);
                }
            };

            return newHandler;
        }

        //public static NotifyCollectionChangedEventHandler MakeWeak(NotifyCollectionChangedEventHandler handler, Action<NotifyCollectionChangedEventHandler> remove)
        //{
        //    var reference = new WeakReference(handler.Target);
        //    var method = handler.Method;

        //    NotifyCollectionChangedEventHandler newHandler = null;
        //    newHandler = (sender, e) =>
        //    {
        //        var target = reference.Target;
        //        if (target != null)
        //        {
        //            method.Invoke(target, new object[] { sender, e });
        //        }
        //        else
        //        {
        //            remove(newHandler);
        //        }
        //    };

        //    return newHandler;
        //}
    }
}
