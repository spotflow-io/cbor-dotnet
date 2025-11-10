using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborTimeOnlyConverter() : CborDateTimeConverterBase<TimeOnly>(supportsWritingDateTimeStringTag: false)
{
    protected override TimeOnly ConvertFromDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return TimeOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    protected override TimeOnly ConvertFromStringWithoutTag(string dateString)
    {
        if (!TimeOnly.TryParse(dateString, CultureInfo.InvariantCulture, out var result))
        {
            throw new CborSerializerException($"The text string '{dateString}' could not be parsed as TimeOnly.");
        }

        return result;
    }

    protected override string FormatToString(TimeOnly value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 format
    }
}
