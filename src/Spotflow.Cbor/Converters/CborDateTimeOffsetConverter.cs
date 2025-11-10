using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborDateTimeOffsetConverter() : CborDateTimeConverterBase<DateTimeOffset>(supportsWritingDateTimeStringTag: true)
{
    protected override DateTimeOffset ConvertFromDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset;
    }

    protected override DateTimeOffset ConvertFromStringWithoutTag(string dateString)
    {
        if (!DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            throw new CborSerializerException($"The text string '{dateString}' could not be parsed as DateTimeOffset.");
        }

        return result;
    }

    protected override string FormatToString(DateTimeOffset value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 / RFC 3339
    }
}
