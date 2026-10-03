using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRT.Core.Json
{
    /// <summary>
    /// Product DateTime as Iran wall-clock without Z/offset.
    /// Kind.Utc is converted to local on write; Z/offset input is converted to local then Unspecified.
    /// </summary>
    public sealed class LocalDateTimeJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTime?);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(DateTime?))
                return new NullableConverter();

            return new DateTimeConverter();
        }

        internal static DateTime ParseLocalWallClock(string text)
        {
            text = text.Trim();

            if (HasExplicitOffset(text)
                && DateTimeOffset.TryParse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dto))
            {
                return DateTime.SpecifyKind(dto.ToLocalTime().DateTime, DateTimeKind.Unspecified);
            }

            var formats = new[]
            {
                "yyyy-MM-dd'T'HH:mm:ss",
                "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF",
                "yyyy-MM-dd'T'HH:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(
                    text,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
            }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);

            throw new JsonException($"Invalid DateTime: '{text}'.");
        }

        internal static void WriteLocalWallClock(Utf8JsonWriter writer, DateTime value)
        {
            var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
            writer.WriteStringValue(local.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private static bool HasExplicitOffset(string text)
        {
            if (text.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                return true;

            return text.Length >= 6
                && (text[^6] == '+' || text[^6] == '-')
                && char.IsDigit(text[^5]);
        }

        private sealed class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException("DateTime must be a JSON string.");

                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                    throw new JsonException("DateTime string is empty.");

                return ParseLocalWallClock(text);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
                => WriteLocalWallClock(writer, value);
        }

        private sealed class NullableConverter : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;

                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException("DateTime must be a JSON string.");

                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                return ParseLocalWallClock(text);
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value is null)
                {
                    writer.WriteNullValue();
                    return;
                }

                WriteLocalWallClock(writer, value.Value);
            }
        }
    }
}