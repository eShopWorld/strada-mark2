using System;

namespace Strada.Api
{
    public class EventTransmittedEventArgs : EventArgs
    {
        public EventTransmittedEventArgs(int numItemsTransmitted)
        {
            NumItemsTransferred = numItemsTransmitted;
        }

        public int NumItemsTransferred { get; }
    }
}