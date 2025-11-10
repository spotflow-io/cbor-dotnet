using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborDateTimeConverter : CborDateTimeConverterBase<DateTime>
{
    protected override bool TryParseFromString(string dateString, out DateTime result)
    {
        return DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
    }

    protected override DateTime ConvertFromUnixTimeSeconds(long seconds, long ticks = 0)
    {
        var baseDateTime = DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
        return baseDateTime.AddTicks(ticks);
    }

    protected override string FormatToString(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 / RFC 3339
    }
}
