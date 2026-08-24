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
        MtsId
    }
}
