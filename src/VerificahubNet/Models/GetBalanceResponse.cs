using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The account balance (<c>GET /v1/balance</c>).</summary>
    public sealed class GetBalanceResponse
    {
        /// <summary>The current account balance.</summary>
        [JsonPropertyName("balance")]
        public MoneyResponse Balance { get; set; } = new MoneyResponse();

        /// <summary>
        /// How far below zero the balance may go, or null when it may not.
        /// </summary>
        /// <remarks>
        /// Set only for accounts with an agreed credit limit; everything else stops at zero.
        /// </remarks>
        [JsonPropertyName("credit_limit")]
        public MoneyResponse? CreditLimit { get; set; }

        /// <summary>
        /// What is actually left to spend: <c>balance</c> plus any credit limit.
        /// </summary>
        /// <remarks>
        /// <b>Check this, not <see cref="Balance"/>.</b> A request costing more than this is
        /// refused with <c>insufficient_balance</c>, and for a credited account the two differ.
        /// </remarks>
        [JsonPropertyName("available")]
        public MoneyResponse Available { get; set; } = new MoneyResponse();
    }
}
