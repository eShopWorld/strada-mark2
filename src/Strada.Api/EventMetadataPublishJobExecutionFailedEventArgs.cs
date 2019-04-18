using System;

namespace Strada.Api
{
    public class EventMetadataPublishJobExecutionFailedEventArgs : EventArgs
    {
        public EventMetadataPublishJobExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}