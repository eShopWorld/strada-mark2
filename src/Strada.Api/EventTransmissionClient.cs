using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Auth;
using Newtonsoft.Json;

namespace Strada.Api
{
    public class EventTransmissionClient
    {
        private static readonly Lazy<EventTransmissionClient> InnerEventTransmissionClient =
            new Lazy<EventTransmissionClient>(() => new EventTransmissionClient());

        private PublisherClient _publisher;
        private TopicName _topicName;

        public static EventTransmissionClient Instance => InnerEventTransmissionClient.Value;

        public bool Initialised { get; private set; }

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;

        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;

        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public async Task InitAsync(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            if (cloudServiceCredentials == null)
                throw new ArgumentNullException(nameof(cloudServiceCredentials));
            if (eventTransmissionClientConfigSettings == null)
                throw new ArgumentNullException(nameof(eventTransmissionClientConfigSettings));
            if (string.IsNullOrEmpty(eventTransmissionClientConfigSettings.ProjectId))
                throw new ArgumentNullException(nameof(eventTransmissionClientConfigSettings.ProjectId));
            if (string.IsNullOrEmpty(eventTransmissionClientConfigSettings.TopicId))
                throw new ArgumentNullException(nameof(eventTransmissionClientConfigSettings.TopicId));

            try
            {
                var credential = GoogleCredential
                    .FromJson(JsonConvert.SerializeObject(cloudServiceCredentials))
                    .CreateScoped(PublisherServiceApiClient.DefaultScopes);

                var settings = new PublisherClient.Settings
                {
                    BatchingSettings = new BatchingSettings(
                        eventTransmissionClientConfigSettings.ElementCountThreshold,
                        eventTransmissionClientConfigSettings.RequestByteThreshold,
                        TimeSpan.FromSeconds(eventTransmissionClientConfigSettings.DelayThreshold))
                };

                var clientCreationSettings = new PublisherClient.ClientCreationSettings(
                    null,
                    null,
                    credential.ToChannelCredentials());

                _topicName = new TopicName(
                    eventTransmissionClientConfigSettings.ProjectId,
                    eventTransmissionClientConfigSettings.TopicId);

                _publisher = await PublisherClient.CreateAsync(_topicName, clientCreationSettings, settings);

                Initialised = true;
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while initializing the data transmission client.";
                OnInitialisationFailed(
                    new EventTransmissionClientInitialisationFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        public async Task TransmitAsync(IEnumerable<string> eventMetadataPayloadBatch)
        {
            if (eventMetadataPayloadBatch == null)
                throw new ArgumentNullException(nameof(eventMetadataPayloadBatch));
            try
            {
                var publishTasks = eventMetadataPayloadBatch
                    .Select(eventMetadataPayload => new PubsubMessage
                        {Data = ByteString.CopyFromUtf8(eventMetadataPayload)})
                    .Select(pubsubMessage => _publisher.PublishAsync(pubsubMessage)).ToList();

                foreach (var publishTask in publishTasks) await publishTask;

                OnDataTransmitted(new EventTransmittedEventArgs(publishTasks.Count));
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while transmitting the payload.";
                OnTransmissionFailed(
                    new EventTransmissionFailedEventArgs(new Exception(errorMessage,
                        exception)));
            }
        }

        private void OnTransmissionFailed(EventTransmissionFailedEventArgs e)
        {
            TransmissionFailed?.Invoke(this, e);
        }

        private void OnInitialisationFailed(EventTransmissionClientInitialisationFailedEventArgs e)
        {
            InitialisationFailed?.Invoke(this, e);
        }

        private void OnDataTransmitted(EventTransmittedEventArgs e)
        {
            DataTransmitted?.Invoke(this, e);
        }
    }
}