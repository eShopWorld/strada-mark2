using System;

namespace Strada.Api
{
    public class EventTransmissionClientInitialisationFailedEventArgs : EventArgs
    {
        public EventTransmissionClientInitialisationFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}