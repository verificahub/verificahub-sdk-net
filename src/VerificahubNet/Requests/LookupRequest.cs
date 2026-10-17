using System.Text.Json.Serialization;

namespace VerificahubNet.Requests
{
    /// <summary>The number to look up.</summary>
    public sealed class LookupRequest
    {
        /// <summary>
        /// The subscriber number in E.164 (<c>+7…</c>). Russian numbers only.
        /// </summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
