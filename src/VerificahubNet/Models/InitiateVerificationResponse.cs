using System;
using System.Text.Json.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>
    /// The result of initiating a verification. The concrete type is chosen by the <c>method</c>
    /// discriminator: <see cref="ReverseFlashCallInitiateResponse"/>, <see cref="SmsInitiateResponse"/>,
    /// <see cref="FlashCallInitiateResponse"/>, or <see cref="TelegramInitiateResponse"/>.
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
}
