using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary.ApplicationModel.Eventing
{
    /// <summary>
    /// 事件订阅对象接受是否可重复
    /// </summary>
    public enum ReceiveType
    {
        //重复接受
        Repeat,
        //对象只接受一次
        Single
    }
}
