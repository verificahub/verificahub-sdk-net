using System;
using System.Text.Json.Serialization;
namespace VerificahubNet.Models
{
    /// <summary>
    /// What a number lookup found (<c>POST /v1/lookup/number-info</c>, <c>POST /v1/lookup/activity</c>).
    /// </summary>
    /// <remarks>
    /// Which fields carry data depends on the service: <see cref="LookupMethod.NumberInfo"/> fills
    /// <see cref="Operator"/>, <see cref="RegionCode"/> and <see cref="Ported"/>, while
    /// <see cref="LookupMethod.ActivityScore"/> fills <see cref="Score"/>. Check
    /// <see cref="Status"/> before reading any of them — when the registry had nothing, they are
    /// all null and <see cref="Cost"/> is zero.
    /// </remarks>
    public sealed class LookupResponse
    {
        /// <summary>Identifier of this lookup, for your own records and for support.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The number that was looked up, masked (<c>+7913*****01</c>).</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Which service answered.</summary>
        [JsonPropertyName("method")]
        public LookupMethod Method { get; set; }

        /// <summary>
        /// Whether the registry had an answer. <see cref="LookupStatus.NoData"/> and
        /// <see cref="LookupStatus.OutOfCoverage"/> are answers too, and neither is charged.
        /// </summary>
        [JsonPropertyName("status")]
        public LookupStatus Status { get; set; }

        /// <summary>The operator currently serving the number. Null unless the service reports one.</summary>
        [JsonPropertyName("operator")]
        public MobileOperator? Operator { get; set; }

        /// <summary>The number's region code. Null unless the service reports one.</summary>
        [JsonPropertyName("region_code")]
        public int? RegionCode { get; set; }

        /// <summary>
        /// How active the number is, from 0 to 1 — only for <see cref="LookupMethod.ActivityScore"/>.
        /// </summary>
        /// <remarks>
        /// Treat it as a signal, not a verdict: the same number can score differently on repeated
        /// lookups, so it is sound for cleaning a list and unsound as the sole basis for refusing
        /// a customer.
        /// </remarks>
        [JsonPropertyName("score")]
        public decimal? Score { get; set; }

        /// <summary>
        /// Whether the number has been ported between operators. Null unless the service reports it.
        /// </summary>
        [JsonPropertyName("ported")]
        public bool? Ported { get; set; }

        /// <summary>
        /// What this lookup cost — <b>zero</b> when no data was found.
        /// </summary>
        /// <remarks>
        /// Reconcile your invoice against this rather than against the number of requests you made.
        /// </remarks>
        [JsonPropertyName("cost")]
        public MoneyResponse Cost { get; set; } = new MoneyResponse();

        /// <summary>When the lookup ran (UTC).</summary>
        [JsonPropertyName("checked_at")]
        public DateTimeOffset CheckedAt { get; set; }
    }
}
