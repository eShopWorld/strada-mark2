namespace Strada.Api
{
    public class EventHandlers
    {
        public delegate void AddEventMetaFailedEventHandler(object sender, AddEventMetaFailedEventArgs e);

        public delegate void ClearCacheFailedEventHandler(object sender, ClearCacheFailedEventArgs e);

        public delegate void DataTransmittedEventHandler(object sender, EventTransmittedEventArgs e);

        public delegate void EventMetaAddedEventHandler(object sender, EventMetaAddedEventArgs e);

        public delegate void GetEventMetadataPayloadBatchEventHandler(
            object sender,
            GetEventMetadataPayloadBatchEventArgs e);

        public delegate void GetEventMetadataPayloadBatchFailedEventHandler(
            object sender,
            GetEventMetadataPayloadBatchFailedEventArgs e);

        public delegate void InitialisationFailedEventHandler(object sender,
            EventTransmissionClientInitialisationFailedEventArgs e);

        public delegate void TransmissionFailedEventHandler(object sender, EventTransmissionFailedEventArgs e);
    }
}