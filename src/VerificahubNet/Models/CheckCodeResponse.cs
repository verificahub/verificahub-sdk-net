using System;
using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The result of submitting a verification code (<c>POST /v1/verify/check</c>).</summary>
    public sealed class CheckCodeResponse
    {
        /// <summary>The unique identifier of the verification.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The verification status (<see cref="VerificationStatus.Verified"/> on success).</summary>
        [JsonPropertyName("status")]
        public VerificationStatus Status { get; set; }

        /// <summary>The masked phone number being verified.</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
