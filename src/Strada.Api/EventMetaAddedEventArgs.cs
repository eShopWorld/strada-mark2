using System;

namespace Strada.Api
{
    public class EventMetaAddedEventArgs : EventArgs
    {
        public EventMetaAddedEventArgs(object eventMeta)
        {
            EventMeta = eventMeta;
        }

        public object EventMeta { get; }
    }
}