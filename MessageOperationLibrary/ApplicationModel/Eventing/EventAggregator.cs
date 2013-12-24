using System;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    public class EventAggregator : IEventAggregator
    {
        /// <summary>
        /// 事件导航
        /// </summary>
        private readonly EventHandlerMap _handlers = new EventHandlerMap();

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="e"></param>
        public void Publish<TEvent>(TEvent e) where TEvent : IEvent
        {

            foreach (var handler in _handlers.GetHandlers<TEvent>())
            {
                //事件触发
                handler.Invoke(e);
            }
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"> </param>
        /// <param name="receiveType"> </param>
        public void Subscribe<TEvent>(Action<TEvent> callback, ReceiveType receiveType) where TEvent : IEvent
        {
            Requires.NotNull(callback, "callback");
            //接收
            _handlers.Register(callback, receiveType);
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="callback"> </param>
        /// <param name="receiveType"> </param>
        public void SubscribeAll(Action<IEvent> callback, ReceiveType receiveType)
        {
            Requires.NotNull(callback, "callback");
            //接收
            _handlers.Register(callback, receiveType);
        }

        /// <summary>
        ///  移除事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"></param>
        public void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : IEvent
        {
            Requires.NotNull(callback, "callback");

            _handlers.Unregister(callback);
        }

        /// <summary>
        /// 移除所有事件
        /// </summary>
        /// <param name="callback"> </param>
        public void UnsubscribeAll(Action<IEvent> callback)
        {
            Requires.NotNull(callback, "callback");

            _handlers.Unregister(callback);
        }



    }
}
