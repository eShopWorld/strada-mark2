using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace Strada.Api
{
    internal class EventMetadataPublishJobListener : IJobListener
    {
        public Task JobToBeExecuted(IJobExecutionContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (jobException != null)
                OnEventMetadataUploadJobExecutionFailed(
                    new EventMetadataPublishJobExecutionFailedEventArgs(jobException));
            return Task.CompletedTask;
        }

        public string Name => "eventMetadataUploadJobListener";

        public event EventMetadataUploadJobExecutionFailedEventHandler EventMetadataUploadJobExecutionFailed;

        protected virtual void OnEventMetadataUploadJobExecutionFailed(
            EventMetadataPublishJobExecutionFailedEventArgs e)
        {
            EventMetadataUploadJobExecutionFailed?.Invoke(this, e);
        }
    }
}