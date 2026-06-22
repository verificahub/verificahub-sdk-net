using System;
using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The polled status of a verification (<c>GET /v1/verify/{request_id}</c>).</summary>
    public sealed class GetVerificationStatusResponse
    {
        /// <summary>The unique identifier of the verification.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The masked phone number being verified (for example <c>+7999*****67</c>).</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>The verification method.</summary>
        [JsonPropertyName("method")]
        public VerificationMethod Method { get; set; }

        /// <summary>The current verification status.</summary>
        [JsonPropertyName("status")]
        public VerificationStatus Status { get; set; }

        /// <summary>The amount charged for the verification.</summary>
        [JsonPropertyName("cost")]
        public MoneyResponse Cost { get; set; } = new MoneyResponse();

        /// <summary>When the verification was created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>When the verification was completed, or <c>null</c> if not yet verified.</summary>
        [JsonPropertyName("verified_at")]
        public DateTimeOffset? VerifiedAt { get; set; }

        /// <summary>When the verification expires.</summary>
        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
