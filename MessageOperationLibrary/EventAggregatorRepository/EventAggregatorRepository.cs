using MessageOperationLibrary.ApplicationModel.Eventing;

namespace MessageOperationLibrary.EventAggregatorRepository
{
    public class MessageEventAggregatorRepository
    {
        private static IEventAggregator _eventAggregator;
        public static IEventAggregator EventAggregator
        {
            get { return _eventAggregator ?? (_eventAggregator = new EventAggregator()); }
        }
    }
}
