using System.Text.Json.Serialization;
using VerificahubNet.Models;

namespace VerificahubNet.Requests
{
    /// <summary>Parameters for initiating a verification (<c>POST /v1/verify</c>).</summary>
    public sealed class InitiateVerificationRequest
    {
        /// <summary>Creates a request for the given phone number using the default method.</summary>
        /// <param name="phoneNumber">The phone number to verify, in E.164 format (for example <c>+79991234567</c>).</param>
        public InitiateVerificationRequest(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        /// <summary>The phone number to verify, in E.164 format.</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>The verification method. Defaults to <see cref="VerificationMethod.ReverseFlashCall"/>.</summary>
        [JsonPropertyName("method")]
        public VerificationMethod Method { get; set; } = VerificationMethod.ReverseFlashCall;

        /// <summary>
        /// Optional. How long the verification stays valid, in seconds (1–600). When omitted the
        /// server applies its default (300).
        /// </summary>
        [JsonPropertyName("expiry_seconds")]
        public int? ExpirySeconds { get; set; }
    }
}
