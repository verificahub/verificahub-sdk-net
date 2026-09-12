using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// The live per-method verification prices for your account currency (<c>GET /v1/prices</c>). Only the
    /// client-facing charge is returned — never the internal provider cost.
    /// </summary>
    public sealed class GetPricesResponse
    {
        /// <summary>One entry per method available to your account.</summary>
        [JsonPropertyName("prices")]
        public IReadOnlyList<MethodPriceResponse> Prices { get; set; } = new List<MethodPriceResponse>();
    }

    /// <summary>The client-facing price for a single verification method.</summary>
    public sealed class MethodPriceResponse
    {
        /// <summary>The verification method this price applies to.</summary>
        [JsonPropertyName("method")]
        public VerificationMethod Method { get; set; }

        /// <summary>The price charged for one verification with this method.</summary>
        [JsonPropertyName("price")]
        public MoneyResponse Price { get; set; } = new MoneyResponse();
    }
}
