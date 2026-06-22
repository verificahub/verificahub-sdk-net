using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VerificahubNet.Serialization
{
    /// <summary>
    /// Serializes enum values to/from their <c>snake_case</c> string representation, matching the
    /// values used by the Verificahub API (for example <c>reverse_flash_call</c>). Unrecognized
    /// strings deserialize to the enum's <c>Unknown</c> member (value 0) for forward-compatibility.
    /// Enums annotated with this converter may also be used as dictionary keys.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert.</typeparam>
    public sealed class SnakeCaseEnumConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        private readonly Dictionary<string, TEnum> _fromString;
        private readonly Dictionary<TEnum, string> _toString;

        /// <summary>Initializes the converter and builds the name lookup tables.</summary>
        public SnakeCaseEnumConverter()
        {
            _fromString = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            _toString = new Dictionary<TEnum, string>();

            foreach (TEnum value in (TEnum[])Enum.GetValues(typeof(TEnum)))
            {
                string name = ToSnakeCase(value.ToString());
                _fromString[name] = value;
                _toString[value] = name;
            }
        }

        /// <inheritdoc />
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Parse(reader.GetString());

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            => writer.WriteStringValue(Format(value));

        /// <inheritdoc />
        public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Parse(reader.GetString());

        /// <inheritdoc />
        public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            => writer.WritePropertyName(Format(value));

        private TEnum Parse(string? raw)
            => raw != null && _fromString.TryGetValue(raw, out TEnum value) ? value : default;

        private string Format(TEnum value)
            => _toString.TryGetValue(value, out string? name) ? name : ToSnakeCase(value.ToString());

        private static string ToSnakeCase(string name)
        {
            var sb = new StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        sb.Append('_');
                    }
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
