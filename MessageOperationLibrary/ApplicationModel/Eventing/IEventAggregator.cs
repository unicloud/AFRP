using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    public interface IEventAggregator
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="e"></param>
        void Publish<TEvent>(TEvent e)
          where TEvent : IEvent;

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"> </param>
        /// <param name="receiveType"> </param>
        void Subscribe<TEvent>(Action<TEvent> callback, ReceiveType receiveType)
            where TEvent : IEvent;

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="callback"> </param>
        /// <param name="receiveType"> </param>
        void SubscribeAll(Action<IEvent> callback, ReceiveType receiveType);
        /// <summary>
        ///  移除事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"></param>
        void Unsubscribe<TEvent>(Action<TEvent> callback)
            where TEvent : IEvent;

        /// <summary>
        /// 移除所有事件
        /// </summary>
        /// <param name="callback"> </param>
        void UnsubscribeAll(Action<IEvent> callback);
    }
}
