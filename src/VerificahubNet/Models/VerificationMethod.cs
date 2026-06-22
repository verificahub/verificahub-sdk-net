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
        TelegramOtp
    }
}
