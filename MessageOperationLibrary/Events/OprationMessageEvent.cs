using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageOperationLibrary.ApplicationModel.Eventing;

namespace MessageOperationLibrary.Events
{
    public class MessageEvent : IEvent
    {
        public MessageEvent()
        {
             
        }
        /// <summary>
        /// 通知的内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 消息通知类型
        /// </summary>
        public MessageType MessageType { get; set; }
    }
    public enum MessageType
    {
        Success,
        Fail,
        Info
    }


    public class NavigationEvent : IEvent
    {
        public NavigationEvent()
        {

        }

        public NavigationEvent(bool skip)
        {
            this.IsSkip = skip;
        }

        //引进方式
        private bool _isSkip = true;
        public bool IsSkip
        {
            get { return this._isSkip; }
            set
            {
                this._isSkip = value;
            }
        }
    }

    public class VersionUpdateEvent : IEvent
    {
        public VersionUpdateEvent()
        {

        }
        /// <summary>
        /// 通知的内容
        /// </summary>
        public string Message { get; set; }
    }
}
