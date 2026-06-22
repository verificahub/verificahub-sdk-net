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
    public sealed class InitiateVerificationResponseConverter : JsonConverter<InitiateVerificationResponse>
    {
        /// <inheritdoc />
        public override InitiateVerificationResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;

            string? method = root.TryGetProperty("method", out JsonElement methodElement)
                ? methodElement.GetString()
                : null;

            Type target = method switch
            {
                "reverse_flash_call" => typeof(ReverseFlashCallInitiateResponse),
                "sms" => typeof(SmsInitiateResponse),
                "flash_call" => typeof(FlashCallInitiateResponse),
                "telegram_otp" => typeof(TelegramInitiateResponse),
                _ => throw new JsonException($"Unknown or missing verification method discriminator: '{method ?? "<null>"}'.")
            };

            // The converter matches only the base type, so deserializing the concrete derived type
            // here uses the default object converter — no recursion.
            var result = (InitiateVerificationResponse?)root.Deserialize(target, options);
            if (result == null)
            {
                throw new JsonException("The verification response body was null.");
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
