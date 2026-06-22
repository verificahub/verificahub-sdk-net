using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>A monetary amount with its ISO-4217 currency code.</summary>
    public sealed class MoneyResponse
    {
        /// <summary>The amount, in major currency units.</summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>The ISO-4217 currency code (v1 is always <c>RUB</c>).</summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "RUB";
    }
}
