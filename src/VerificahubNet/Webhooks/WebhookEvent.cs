using System;
using System.Text.Json.Serialization;
using VerificahubNet.Models;

namespace VerificahubNet.Webhooks
{
    /// <summary>
    /// The payload Verificahub POSTs to a configured webhook URL on a verification state change.
    /// See <see cref="WebhookEventTypes"/> for the <see cref="Event"/> values.
    /// </summary>
    public sealed class WebhookEvent
    {
        /// <summary>The event name (for example <c>verification.verified</c>).</summary>
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        /// <summary>The verification identifier.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The masked phone number (for example <c>+7999*****67</c>).</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>The verification method.</summary>
        [JsonPropertyName("method")]
        public VerificationMethod Method { get; set; }

        /// <summary>The verification status at the time of the event.</summary>
        [JsonPropertyName("status")]
        public VerificationStatus Status { get; set; }

        /// <summary>When the event was emitted (UTC).</summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
