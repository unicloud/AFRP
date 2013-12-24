using System;
using Microsoft.Practices.Prism.Events;

namespace UniCloud.Infrastructure
{
    public class OprationMessageEvent : CompositePresentationEvent<Tuple<OperationMessageType, string>> { }


    public enum OperationMessageType { Success, Fail, Info }
}