using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Strada.Api
{
    internal class JobScheduler
    {
        public delegate void JobSchedulerStartFailedEventHandler(object sender, JobSchedulerStartFailedEventArgs e);

        private static readonly Lazy<JobScheduler> InnerDataUploader =
            new Lazy<JobScheduler>(() => new JobScheduler());

        private EventMetadataPublishJobListener _eventMetadataUploadJobListener;
        private JobDetailImpl _jobDetail;

        private IScheduler _scheduler;
        private ITrigger _trigger;

        public static JobScheduler Instance => InnerDataUploader.Value;

        public event JobSchedulerStartFailedEventHandler DataUploaderStartFailed;

        public event EventMetadataUploadJobExecutionFailedEventHandler EventMetadataUploadJobExecutionFailed;

        public async Task StartAsync(
            EventTransmissionClient eventTransmissionClient,
            EventMetaCache eventMetaCache,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            if (eventTransmissionClient == null) throw new ArgumentNullException(nameof(eventTransmissionClient));
            if (eventMetaCache == null) throw new ArgumentNullException(nameof(eventMetaCache));
            if (eventTransmissionClientConfigSettings == null)
                throw new ArgumentNullException(nameof(eventTransmissionClientConfigSettings));

            try
            {
                eventMetaCache.MaxQueueLength = eventTransmissionClientConfigSettings.MaxQueueLength;

                var factory = new StdSchedulerFactory(new NameValueCollection
                {
                    ["quartz.threadPool.threadCount"] = eventTransmissionClientConfigSettings.MaxThreadCount.ToString()
                });
                _scheduler = await factory.GetScheduler();

                await _scheduler.Start();

                const string jobName = "eventMetadataUploadJob";

                _jobDetail = new JobDetailImpl(
                    jobName,
                    typeof(EventMetadataPublishJob))
                {
                    JobDataMap =
                    {
                        [nameof(EventTransmissionClient)] = eventTransmissionClient,
                        [nameof(EventMetaCache)] = eventMetaCache
                    }
                };

                _eventMetadataUploadJobListener = new EventMetadataPublishJobListener();

                if (EventMetadataUploadJobExecutionFailed != null)
                    _eventMetadataUploadJobListener.EventMetadataUploadJobExecutionFailed +=
                        EventMetadataUploadJobExecutionFailed;

                _scheduler.ListenerManager.AddJobListener(
                    _eventMetadataUploadJobListener,
                    KeyMatcher<JobKey>.KeyEquals(new JobKey(jobName)));

                _trigger = TriggerBuilder.Create()
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(eventTransmissionClientConfigSettings.ExecutionTimeInterval)
                        .RepeatForever())
                    .Build();

                await _scheduler.ScheduleJob(_jobDetail, _trigger);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Failed to start data-upload background task.";
                OnDataUploaderStartFailed(new JobSchedulerStartFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        private void OnDataUploaderStartFailed(JobSchedulerStartFailedEventArgs e)
        {
            DataUploaderStartFailed?.Invoke(this, e);
        }
    }
}