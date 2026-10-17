using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>A usage summary over a date range (<c>GET /v1/usage</c>).</summary>
    public sealed class GetUsageResponse
    {
        /// <summary>The first day of the reporting range (inclusive).</summary>
        [JsonPropertyName("from")]
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTime From { get; set; }

        /// <summary>The last day of the reporting range (inclusive).</summary>
        [JsonPropertyName("to")]
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTime To { get; set; }

        /// <summary>The total number of verifications initiated in the range.</summary>
        [JsonPropertyName("total_verifications")]
        public int TotalVerifications { get; set; }

        /// <summary>The number of verifications that completed successfully.</summary>
        [JsonPropertyName("successful")]
        public int Successful { get; set; }

        /// <summary>The success rate, between 0.0 and 1.0.</summary>
        [JsonPropertyName("success_rate")]
        public double SuccessRate { get; set; }

        /// <summary>The total amount charged across the range.</summary>
        [JsonPropertyName("total_cost")]
        public MoneyResponse TotalCost { get; set; } = new MoneyResponse();

        /// <summary>Verification counts grouped by method.</summary>
        [JsonPropertyName("by_method")]
        public IReadOnlyDictionary<VerificationMethod, int> ByMethod { get; set; }
            = new Dictionary<VerificationMethod, int>();

        /// <summary>Verification counts grouped by final status.</summary>
        [JsonPropertyName("by_status")]
        public IReadOnlyDictionary<VerificationStatus, int> ByStatus { get; set; }
            = new Dictionary<VerificationStatus, int>();

        /// <summary>Per-day breakdown; days with no activity are omitted.</summary>
        [JsonPropertyName("daily")]
        public IReadOnlyList<UsageDayResponse> Daily { get; set; } = Array.Empty<UsageDayResponse>();

        /// <summary>
        /// Number-lookup usage over the same range, counted separately from verifications.
        /// </summary>
        /// <remarks>
        /// The fields above cover verifications only, so what you spent over the range is
        /// <c>total_cost</c> <b>plus</b> <c>lookups.total_cost</c>.
        /// </remarks>
        [JsonPropertyName("lookups")]
        public LookupUsageResponse Lookups { get; set; } = new LookupUsageResponse();
    }

    /// <summary>A single day's verification counts within a usage report.</summary>
    public sealed class UsageDayResponse
    {
        /// <summary>The calendar day.</summary>
        [JsonPropertyName("date")]
        [JsonConverter(typeof(IsoDateConverter))]
        public DateTime Date { get; set; }

        /// <summary>The total number of verifications initiated that day.</summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>The number of verifications that completed successfully that day.</summary>
        [JsonPropertyName("verified")]
        public int Verified { get; set; }
    }

    /// <summary>Number-lookup usage within a usage report.</summary>
    public sealed class LookupUsageResponse
    {
        /// <summary>Lookups requested in the range, answered or not.</summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// How many of those were charged for — lower than <see cref="Total"/> by exactly the ones
        /// that found no data, which are free. This is the count <see cref="TotalCost"/> is made of.
        /// </summary>
        [JsonPropertyName("billable")]
        public int Billable { get; set; }

        /// <summary>What the lookups cost in total.</summary>
        [JsonPropertyName("total_cost")]
        public MoneyResponse TotalCost { get; set; } = new MoneyResponse();

        /// <summary>Counts per lookup service.</summary>
        [JsonPropertyName("by_method")]
        public IReadOnlyDictionary<LookupMethod, int> ByMethod { get; set; } = new Dictionary<LookupMethod, int>();

        /// <summary>Counts per outcome — where the share of "no data" shows up.</summary>
        [JsonPropertyName("by_status")]
        public IReadOnlyDictionary<LookupStatus, int> ByStatus { get; set; } = new Dictionary<LookupStatus, int>();
    }

}
