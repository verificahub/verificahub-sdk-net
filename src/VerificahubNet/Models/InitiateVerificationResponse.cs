using System;
using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// The result of initiating a verification. The concrete type is chosen by the <c>method</c>
    /// discriminator: <see cref="ReverseFlashCallInitiateResponse"/>, <see cref="SmsInitiateResponse"/>,
    /// <see cref="FlashCallInitiateResponse"/>, <see cref="TelegramInitiateResponse"/>,
    /// <see cref="MtsIdInitiateResponse"/>, or <see cref="MaxBotInitiateResponse"/>.
    /// </summary>
    public abstract class InitiateVerificationResponse
    {
        /// <summary>The verification method that produced this response.</summary>
        [JsonIgnore]
        public abstract VerificationMethod Method { get; }

        /// <summary>The unique identifier of the verification.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The phone number being verified, in E.164 format.</summary>
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>The current verification status (initially <see cref="VerificationStatus.Sent"/>).</summary>
        [JsonPropertyName("status")]
        public VerificationStatus Status { get; set; }

        /// <summary>The amount charged (or to be charged) for the verification.</summary>
        [JsonPropertyName("cost")]
        public MoneyResponse Cost { get; set; } = new MoneyResponse();

        /// <summary>When the verification expires.</summary>
        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>The expected length of the verification code.</summary>
        [JsonPropertyName("code_length")]
        public int CodeLength { get; set; }
    }

    /// <summary>
    /// The <c>reverse_flash_call</c> initiation result: the user must dial <see cref="NumberToCall"/>
    /// from the number being verified, and we auto-verify by caller id.
    /// </summary>
    public sealed class ReverseFlashCallInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.ReverseFlashCall;

        /// <summary>The gateway number the user must dial to complete the verification.</summary>
        [JsonPropertyName("number_to_call")]
        public string NumberToCall { get; set; } = string.Empty;
    }

    /// <summary>The <c>sms</c> initiation result.</summary>
    public sealed class SmsInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.Sms;
    }

    /// <summary>The <c>flash_call</c> initiation result.</summary>
    public sealed class FlashCallInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.FlashCall;
    }

    /// <summary>The <c>telegram_otp</c> initiation result.</summary>
    public sealed class TelegramInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.TelegramOtp;
    }

    /// <summary>
    /// The <c>mts_id</c> initiation result. Nothing is typed on the push leg — MTS confirms silently
    /// on the SIM and the outcome arrives over the notification webhook, so <see cref="InitiateVerificationResponse.Status"/>
    /// starts <see cref="VerificationStatus.Sent"/>. On SMS-OTP fallback a code is expected; poll
    /// <see cref="GetVerificationStatusResponse.AwaitingCode"/> (or handle the
    /// <c>verification.code_required</c> webhook) and submit it with <c>CheckCodeAsync</c>.
    /// </summary>
    public sealed class MtsIdInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.MtsId;
    }

    /// <summary>
    /// The <c>max_bot</c> initiation result. Show the user <see cref="DeepLink"/> (as a link or QR); they
    /// open the MAX bot, share their number, and receive the code in the MAX messenger, then submit it with
    /// <c>CheckCodeAsync</c>. <see cref="InitiateVerificationResponse.Status"/> starts
    /// <see cref="VerificationStatus.Sent"/> and flips to <see cref="VerificationStatus.Delivered"/> once
    /// the code reaches the user (also surfaced by <see cref="GetVerificationStatusResponse.AwaitingCode"/>).
    /// </summary>
    public sealed class MaxBotInitiateResponse : InitiateVerificationResponse
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override VerificationMethod Method => VerificationMethod.MaxBot;

        /// <summary>The MAX deep link to open the bot (<c>https://max.ru/&lt;bot&gt;?start=…</c>).</summary>
        [JsonPropertyName("deep_link")]
        public string DeepLink { get; set; } = string.Empty;
    }
}
