using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborDateTimeConverter() : CborDateTimeConverterBase<DateTime>(supportsWritingDateTimeStringTag: true)
{
    protected override DateTime ConvertFromDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.DateTime;
    }

    protected override DateTime ConvertFromStringWithoutTag(string dateString)
    {
        if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            throw new CborSerializerException($"The text string '{dateString}' could not be parsed as DateTime.");
        }

        return result;
    }

    protected override string FormatToString(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 / RFC 3339
    }
}
