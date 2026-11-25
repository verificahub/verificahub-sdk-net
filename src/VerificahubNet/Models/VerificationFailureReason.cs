using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// Why a verification ended <see cref="VerificationStatus.Expired"/> or
    /// <see cref="VerificationStatus.Failed"/>.
    /// </summary>
    /// <remarks>
    /// The API may add reasons; an unrecognised one deserializes to <see cref="Unknown"/>.
    /// </remarks>
    [JsonConverter(typeof(SnakeCaseEnumConverter<VerificationFailureReason>))]
    public enum VerificationFailureReason
    {
        /// <summary>
        /// The channel did not say why, or the reason is one this SDK version does not recognise.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The user never completed the step before the session ran out.
        /// </summary>
        NoResponse,

        /// <summary>
        /// The user actively refused (for example rejected the confirmation).
        /// </summary>
        /// <remarks>
        /// A refusal is a decision, so retrying the same channel tends to annoy rather than convert.
        /// </remarks>
        Declined,

        /// <summary>
        /// The code was entered wrongly until the attempts ran out.
        /// </summary>
        WrongCode,

        /// <summary>
        /// The code or call never reached the user.
        /// </summary>
        /// <remarks>
        /// Worth retrying on another method.
        /// </remarks>
        NotDelivered,

        /// <summary>
        /// The number cannot be reached on this channel: unknown, switched off, or the channel is
        /// unavailable for it.
        /// </summary>
        Unreachable
    }
}
