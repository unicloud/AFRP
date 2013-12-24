using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{    /// <summary>
    /// 事件导航
    /// </summary>
    public class EventHandlerMap
    {
        /// <summary>
        /// 存在事件
        /// </summary>
        private readonly Dictionary<Type, List<object>> _handlerMap = new Dictionary<Type, List<object>>();
        /// <summary>
        /// 初始化
        /// </summary>
        public EventHandlerMap()
        {

        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"> </param>
        /// <param name="receiveType"> </param>
        public void Register<TEvent>(Action<TEvent> callback, ReceiveType receiveType)
                    where TEvent : IEvent
        {
            var handlers = GetOrCreateHandlers<TEvent>();
            switch (receiveType)
            {
                case ReceiveType.Repeat:
                    handlers.Add(callback);
                    break;
                case ReceiveType.Single:
                    if (!IsExistSameMethodTarget(callback))
                    {
                        handlers.Add(callback);
                    }
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 注销事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"> </param>
        public void Unregister<TEvent>(Action<TEvent> callback)
                where TEvent : IEvent
        {
            var handlers = GetOrCreateHandlers<TEvent>();
            handlers.Remove(callback);
        }
        /// <summary>
        /// 获取事件触发器
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public IEnumerable<Action<TEvent>> GetHandlers<TEvent>()
            where TEvent : IEvent
        {
            var handlers = GetOrCreateHandlers<TEvent>();

            return handlers.Cast<Action<TEvent>>();
        }
        /// <summary>
        /// 获取事件
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private IList<object> GetOrCreateHandlers<TEvent>()
            where TEvent : IEvent
        {

            List<object> handlers;
            if (!_handlerMap.TryGetValue(typeof(TEvent), out handlers))
            {
                handlers = new List<object>();
                _handlerMap.Add(typeof(TEvent), handlers);
            }

            return handlers;
        }
        /// <summary>
        /// 同一个接受对象是否可重复接受
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback"></param>
        /// <returns></returns>
        private bool IsExistSameMethodTarget<TEvent>(Action<TEvent> callback) where TEvent : IEvent
        {
            var handle = GetHandlers<TEvent>();
            return handle.Any(p => p.Method == callback.Method && p.Target == callback.Target);
        }
    }
}
