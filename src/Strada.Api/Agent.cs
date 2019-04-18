#if NET461
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
#endif
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if (NETCOREAPP2_1 || NETCOREAPP2_2)
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#endif

namespace Strada.Api
{
    public class Agent
    {
        private static readonly Lazy<Agent> Lazy =
            new Lazy<Agent>(() => new Agent());

        public static Agent Instance => Lazy.Value;

        public event EventHandlers.EventMetaAddedEventHandler EventMetaAdded;
        public event EventHandlers.AddEventMetaFailedEventHandler AddEventMetaFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchFailedEventHandler GetEventMetadataPayloadBatchFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchEventHandler GotEventMetadataPayloadBatch;
        public event EventHandlers.ClearCacheFailedEventHandler ClearCacheFailed;

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;
        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;
        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public void Start(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
        {
            EventMetaCache.Instance.EventMetaAdded += EventMetaAdded;
            EventMetaCache.Instance.AddEventMetaFailed += AddEventMetaFailed;
            EventMetaCache.Instance.GetEventMetadataPayloadBatchFailed += GetEventMetadataPayloadBatchFailed;
            EventMetaCache.Instance.GotEventMetadataPayloadBatch += GotEventMetadataPayloadBatch;
            EventMetaCache.Instance.ClearCacheFailed += ClearCacheFailed;

            EventTransmissionClient.Instance.InitialisationFailed += InitialisationFailed;
            EventTransmissionClient.Instance.TransmissionFailed += TransmissionFailed;
            EventTransmissionClient.Instance.DataTransmitted += DataTransmitted;

            EventTransmissionClient.Instance.InitAsync(
                cloudServiceCredentials,
                clientConfigSettings
            ).Wait();

            JobScheduler.Instance.StartAsync(
                EventTransmissionClient.Instance,
                EventMetaCache.Instance,
                clientConfigSettings).Wait();
        }

        public void AddEvent<T>(T eventMetadataPayload,
            string brandCode,
            string eventName,
            string fingerprint,
            string queryString = null,
            Dictionary<string, string> httpHeaders = null)
        {
            EventMetaCache.Instance.Add(
                eventMetadataPayload,
                brandCode,
                eventName,
                fingerprint,
                queryString,
                httpHeaders);
        }

        public static string AddTrackingMetadataToJson(
            string json,
            string brandCode,
            string eventName,
            string fingerprint,
            string queryString,
            Dictionary<string, string> httpHeaders,
            string timestamp)
        {
            try
            {
                var jsonObject = JObject.Parse(json);
                jsonObject.Add(new JProperty("brandCode", brandCode));
                jsonObject.Add(new JProperty("eventName", eventName));
                jsonObject.Add(new JProperty("correlationId", fingerprint));
                jsonObject.Add(new JProperty("queryString", queryString));
                jsonObject.Add(httpHeaders != null
                    ? new JProperty("httpHeaders", JObject.FromObject(httpHeaders))
                    : new JProperty("httpHeaders", null));
                jsonObject.Add(new JProperty("created", timestamp));
                return jsonObject.ToString();
            }
            catch (Exception exception)
            {
                throw new JsonSerializationException($"Could not edit the JSON payload: {json}", exception);
            }
        }


        public static string AddTrackingMetadataToJson(
            string json,
            string eventName,
            Dictionary<string, string> httpHeaders,
            string timestamp)
        {
            try
            {
                var jsonObject = JObject.Parse(json);
                jsonObject.Add(new JProperty("eventName", eventName));
                jsonObject.Add(httpHeaders != null
                    ? new JProperty("httpHeaders", JObject.FromObject(httpHeaders))
                    : new JProperty("httpHeaders", null));
                jsonObject.Add(new JProperty("created", timestamp));
                return jsonObject.ToString();
            }
            catch (Exception exception)
            {
                throw new JsonSerializationException($"Could not edit the JSON payload: {json}", exception);
            }
        }


#if (NETCOREAPP2_1 || NETCOREAPP2_2)
        public static Dictionary<string, string>
            ParseHttpHeaders(IHeaderDictionary httpRequestHeaders)
        {
            return httpRequestHeaders?.ToDictionary(h => h.Key, h => h.Value.LastOrDefault());
        }

        public static string GetFingerprint(
            HttpRequest httpRequest,
            string fingerprintHeaderName = "FingerprintId")
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

            var gotFingerprint = httpRequest.Headers.TryGetValue(fingerprintHeaderName, out var headerValues);
            if (!gotFingerprint) return null;

            string fingerprint = null;
            if (!StringValues.IsNullOrEmpty(headerValues))
                fingerprint = headerValues.LastOrDefault();

            return fingerprint;
        }
#endif

#if NET461
        public static Dictionary<string, string>
            ParseHttpHeaders(NameValueCollection httpRequestHeadersCollection)
        {
            return httpRequestHeadersCollection.Cast<string>()
                .ToDictionary(k => k, k => httpRequestHeadersCollection[k]);
        }

        public static Dictionary<string, string>
            ParseHttpHeaders(HttpRequestHeaders httpRequestHeaders)
        {
            if (httpRequestHeaders == null) return null;

            var parsedHttpHeaders = new Dictionary<string, string>();

            foreach (var httpRequestHeader in httpRequestHeaders)
                parsedHttpHeaders.Add(httpRequestHeader.Key, httpRequestHeader.Value.LastOrDefault());

            return parsedHttpHeaders;
        }

        public static string GetFingerprint(
            HttpRequestMessage httpRequest,
            string fingerprintHeaderName = "FingerprintId")
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (httpRequest.Headers == null) throw new ArgumentNullException(nameof(httpRequest.Headers));

            var gotHeaderValues = httpRequest.Headers.TryGetValues(fingerprintHeaderName, out var headerValues);
            return gotHeaderValues ? headerValues.LastOrDefault() : null;
        }
#endif
    }
}