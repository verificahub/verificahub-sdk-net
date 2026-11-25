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
}
