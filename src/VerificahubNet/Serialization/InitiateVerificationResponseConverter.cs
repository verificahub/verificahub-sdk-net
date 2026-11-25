using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using VerificahubNet.Models;

namespace VerificahubNet.Serialization
{
    /// <summary>
    /// Deserializes the polymorphic <see cref="InitiateVerificationResponse"/> by reading the
    /// <c>method</c> discriminator wherever it appears in the JSON object. The built-in
    /// System.Text.Json polymorphism requires the discriminator to be the first property, which the
    /// server does not guarantee; this converter is position-independent.
    /// </summary>
    /// <remarks>
    /// An unrecognised or missing discriminator yields <see cref="UnknownInitiateResponse"/> instead of
    /// throwing. The verification has already been created server-side by the time this runs, so failing
    /// here would hide a live (and possibly billed) verification from the caller.
    /// </remarks>
    public sealed class InitiateVerificationResponseConverter : JsonConverter<InitiateVerificationResponse>
    {
        /// <inheritdoc />
        public override InitiateVerificationResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;

            string? method = root.TryGetProperty("method", out JsonElement methodElement)
                && methodElement.ValueKind == JsonValueKind.String
                    ? methodElement.GetString()
                    : null;

            Type target = method switch
            {
                "reverse_flash_call" => typeof(ReverseFlashCallInitiateResponse),
                "sms" => typeof(SmsInitiateResponse),
                "flash_call" => typeof(FlashCallInitiateResponse),
                "telegram_otp" => typeof(TelegramInitiateResponse),
                "whatsapp_otp" => typeof(WhatsAppInitiateResponse),
                "mts_id" => typeof(MtsIdInitiateResponse),
                "max_bot" => typeof(MaxBotInitiateResponse),
                "pass_call" => typeof(PassCallInitiateResponse),
                _ => typeof(UnknownInitiateResponse)
            };

            // The converter matches only the base type, so deserializing the concrete derived type
            // here uses the default object converter — no recursion.
            var result = (InitiateVerificationResponse?)root.Deserialize(target, options);
            if (result == null)
            {
                throw new JsonException("The verification response body was null.");
            }

            if (result is UnknownInitiateResponse unknown)
            {
                unknown.RawMethod = method;
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, InitiateVerificationResponse value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
