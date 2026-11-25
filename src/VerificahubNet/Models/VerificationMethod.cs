using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The method used to deliver or complete a verification.</summary>
    /// <remarks>
    /// Values are pinned explicitly: members retired from the API leave a gap rather than shifting the
    /// numbers of the ones that remain, so a value you have persisted as an integer keeps its meaning.
    /// </remarks>
    [JsonConverter(typeof(SnakeCaseEnumConverter<VerificationMethod>))]
    public enum VerificationMethod
    {
        /// <summary>An unrecognized or unset method (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>Inbound flash call: the user dials a number we hand out and we verify by caller id (no code to submit).</summary>
        ReverseFlashCall = 1,

        /// <summary>Outbound flash call: the verification code is the trailing digits of the calling number.</summary>
        FlashCall = 2,

        // 3 was Voice, which the API never accepted; the voice method is PassCall.

        /// <summary>A one-time code delivered by SMS.</summary>
        Sms = 4,

        /// <summary>A one-time code delivered over Telegram.</summary>
        TelegramOtp = 5,

        /// <summary>
        /// MTS ID: MTS pushes a silent confirmation to the user's SIM and the result arrives
        /// asynchronously over the notification webhook (no code is typed on the push leg). If the
        /// push cannot be delivered the flow falls back to SMS-OTP, at which point a code is expected
        /// (surfaced by <c>awaiting_code</c> / the <c>verification.code_required</c> webhook) and is
        /// submitted via <c>CheckCodeAsync</c>.
        /// </summary>
        MtsId = 6,

        // 7 was MaxOtp, withdrawn from the public API.

        /// <summary>
        /// MAX bot: we return a MAX deep link (<see cref="MaxBotInitiateResponse.DeepLink"/>). The
        /// user opens the bot and shares their number, the bot delivers the code inside the MAX messenger,
        /// and the user submits it via <c>CheckCodeAsync</c>. The status starts <c>Sent</c> and flips to
        /// <c>Delivered</c> once the code reaches the user.
        /// </summary>
        MaxBot = 8,

        /// <summary>
        /// A one-time code delivered from the official VerificaHub WhatsApp Business account. The user
        /// submits it via <c>CheckCodeAsync</c>, as with <see cref="Sms"/>.
        /// </summary>
        WhatsappOtp = 9,

        /// <summary>
        /// Voice verification: a robot calls the number and dictates the code, which the user submits via
        /// <c>CheckCodeAsync</c>. Charged when the call is requested, not when it is answered, and the call
        /// shows the caller-ID label of the platform's legal entity rather than your service name.
        /// </summary>
        PassCall = 10
    }
}
