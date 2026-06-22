using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// An RFC-7807 problem-details error body returned by the Verificahub API on a non-2xx response.
    /// </summary>
    public sealed class ProblemDetails
    {
        /// <summary>A URI identifying the problem type.</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>A short, human-readable summary of the problem.</summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>The HTTP status code.</summary>
        [JsonPropertyName("status")]
        public int? Status { get; set; }

        /// <summary>A human-readable explanation specific to this occurrence.</summary>
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        /// <summary>A reference identifying the specific occurrence (method + path).</summary>
        [JsonPropertyName("instance")]
        public string? Instance { get; set; }

        /// <summary>The stable machine-readable error code — branch on this, not <see cref="Detail"/>.</summary>
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }

        /// <summary>A correlation/trace identifier for support.</summary>
        [JsonPropertyName("trace_id")]
        public string? TraceId { get; set; }

        /// <summary>For an <c>invalid_code</c> error: the number of code-check attempts remaining.</summary>
        [JsonPropertyName("attempts_remaining")]
        public int? AttemptsRemaining { get; set; }
    }
}
