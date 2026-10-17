using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>What a lookup was able to answer.</summary>
    [JsonConverter(typeof(SnakeCaseEnumConverter<LookupStatus>))]
    public enum LookupStatus
    {
        /// <summary>An unrecognized or unset status (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>The registry answered: the data is on the response.</summary>
        Ok,

        /// <summary>
        /// The registry holds nothing for this number.
        /// </summary>
        /// <remarks>
        /// <b>An answer, not an error.</b> It arrives as a <c>200</c> and is not charged — the
        /// response's <c>cost</c> is zero. Retrying will not change it, and a number with no
        /// activity score may still answer <see cref="LookupMethod.NumberInfo"/>: this describes
        /// the service for that number, not the number.
        /// </remarks>
        NoData,

        /// <summary>
        /// The number is outside the service's operator coverage. Also an answer, also free.
        /// </summary>
        OutOfCoverage
    }
}
