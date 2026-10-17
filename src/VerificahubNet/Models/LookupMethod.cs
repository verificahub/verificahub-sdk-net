using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// A number-lookup service.
    /// </summary>
    /// <remarks>
    /// A lookup is <b>not</b> a verification: it reads what the operator's registry says about a
    /// number and proves nothing about who is holding the phone. It has no session, no code and no
    /// webhook, and it is charged per answer.
    /// </remarks>
    [JsonConverter(typeof(SnakeCaseEnumConverter<LookupMethod>))]
    public enum LookupMethod
    {
        /// <summary>An unrecognized or unset service (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>Operator, region, and whether the number has been ported.</summary>
        NumberInfo,

        /// <summary>How active the number is in the network, scored from 0 to 1.</summary>
        ActivityScore,

        /// <summary>SIM registration status. Defined but not currently sold.</summary>
        SimStatus
    }
}
