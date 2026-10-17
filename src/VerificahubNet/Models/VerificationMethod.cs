using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The method used to deliver or complete a verification.</summary>
    [JsonConverter(typeof(SnakeCaseEnumConverter<VerificationMethod>))]
    public enum VerificationMethod
    {
        /// <summary>An unrecognized or unset method (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>Inbound flash call: the user dials a number we hand out and we verify by caller id (no code to submit).</summary>
        ReverseFlashCall,

        /// <summary>Outbound flash call: the verification code is the trailing digits of the calling number.</summary>
        FlashCall,

        /// <summary>A voice call that reads out the code (not currently offered).</summary>
        Voice,

        /// <summary>A one-time code delivered by SMS.</summary>
        Sms,

        /// <summary>A one-time code delivered over Telegram.</summary>
        TelegramOtp,

        /// <summary>
        /// MTS ID: MTS pushes a silent confirmation to the user's SIM and the result arrives
        /// asynchronously over the notification webhook (no code is typed on the push leg). If the
        /// push cannot be delivered the flow falls back to SMS-OTP, at which point a code is expected
        /// (surfaced by <c>awaiting_code</c> / the <c>verification.code_required</c> webhook) and is
        /// submitted via <c>CheckCodeAsync</c>.
        /// </summary>
        MtsId,

        /// <summary>
        /// MAX: the code is sent straight to the user's number as a MAX message — nothing to open,
        /// no bot and no link. The user submits it via <c>CheckCodeAsync</c>, as with
        /// <see cref="Sms"/>. Delivery is synchronous, so a number that is not on MAX fails
        /// immediately and is not charged. Distinct from <see cref="MaxBot"/>, which is cheaper but
        /// costs the user an extra step.
        /// </summary>
        MaxOtp,

        /// <summary>
        /// MAX bot: we return a MAX deep link (<see cref="MaxBotInitiateResponse.DeepLink"/>). The
        /// user opens the bot and shares their number, the bot delivers the code inside the MAX messenger,
        /// and the user submits it via <c>CheckCodeAsync</c>. The status starts <c>Sent</c> and flips to
        /// <c>Delivered</c> once the code reaches the user.
        /// </summary>
        MaxBot
    }
}
