using System.Text.Json;
using System.Text.Json.Serialization;

namespace VerificahubNet.Serialization
{
    /// <summary>Shared <see cref="JsonSerializerOptions"/> used across the SDK.</summary>
    internal static class VerificahubJson
    {
        // Property names come verbatim from [JsonPropertyName] attributes (no naming policy);
        // null values are omitted when writing request bodies.
        internal static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new InitiateVerificationResponseConverter() }
        };
    }
}
