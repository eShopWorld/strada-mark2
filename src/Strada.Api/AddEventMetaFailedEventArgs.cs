using System;

namespace Strada.Api
{
    public class AddEventMetaFailedEventArgs : EventArgs
    {
        public AddEventMetaFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}