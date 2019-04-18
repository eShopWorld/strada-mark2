using System;
using System.Threading.Tasks;
using Quartz;

namespace Strada.Api
{
    internal class EventMetadataPublishJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var eventMetaCache = (EventMetaCache) dataMap[nameof(EventMetaCache)];
                var eventTransmissionClient = (EventTransmissionClient) dataMap[nameof(EventTransmissionClient)];

                var eventMetadataPayloadBatch = eventMetaCache.GetEventMetadataPayloadBatch();
                await eventTransmissionClient.TransmitAsync(eventMetadataPayloadBatch);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Event meta-upload background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}