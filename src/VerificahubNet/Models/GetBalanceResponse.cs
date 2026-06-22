using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The account balance (<c>GET /v1/balance</c>).</summary>
    public sealed class GetBalanceResponse
    {
        /// <summary>The current account balance.</summary>
        [JsonPropertyName("balance")]
        public MoneyResponse Balance { get; set; } = new MoneyResponse();
    }
}
