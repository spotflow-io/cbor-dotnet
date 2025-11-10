using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborDateTimeOffsetConverter : CborDateTimeConverterBase<DateTimeOffset>
{
    protected override bool TryParseFromString(string dateString, out DateTimeOffset result)
    {
        return DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
    }

    protected override DateTimeOffset ConvertFromUnixTimeSeconds(long seconds, long ticks = 0)
    {
        var baseDateTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
        return baseDateTime.AddTicks(ticks);
    }

    protected override string FormatToString(DateTimeOffset value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 / RFC 3339
    }
}
