using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VerificahubNet.Serialization
{
    /// <summary>
    /// Serializes a <see cref="DateTime"/> to/from an ISO-8601 calendar date (<c>yyyy-MM-dd</c>),
    /// matching the date-only fields on the Verificahub API (for example the usage report's
    /// <c>from</c>/<c>to</c> and daily <c>date</c>). The time component is ignored.
    /// </summary>
    public sealed class IsoDateConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "yyyy-MM-dd";

        /// <inheritdoc />
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? raw = reader.GetString();
            if (raw == null)
            {
                return default;
            }
            if (DateTime.TryParseExact(raw, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exact))
            {
                return exact;
            }
            return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed) ? parsed : default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}
