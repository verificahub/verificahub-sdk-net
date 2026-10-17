using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// The operator serving a number, as the registry reports it.
    /// </summary>
    /// <remarks>
    /// This is the operator the number is on <b>now</b>, which is not what its prefix implies — a
    /// ported number keeps the prefix of the operator that issued it. That is the whole reason to
    /// ask rather than infer.
    /// </remarks>
    [JsonConverter(typeof(SnakeCaseEnumConverter<MobileOperator>))]
    public enum MobileOperator
    {
        /// <summary>An unrecognized or unset operator (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>MTS.</summary>
        Mts,

        /// <summary>MegaFon.</summary>
        Megafon,

        /// <summary>Beeline.</summary>
        Beeline,

        /// <summary>Tele2.</summary>
        Tele2,

        /// <summary>An operator outside the named set, including MVNOs.</summary>
        Other
    }
}
