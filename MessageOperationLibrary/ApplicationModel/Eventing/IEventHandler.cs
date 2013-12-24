using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    /// <summary>
    /// 事件触发句柄
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="e">触发的项</param>
        void Handle(TEvent e);

    }
}
