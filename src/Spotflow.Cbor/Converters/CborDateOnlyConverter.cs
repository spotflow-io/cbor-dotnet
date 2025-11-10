using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborDateOnlyConverter() : CborDateTimeConverterBase<DateOnly>(supportsWritingDateTimeStringTag: false)
{
    protected override DateOnly ConvertFromDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    protected override DateOnly ConvertFromStringWithoutTag(string dateString)
    {
        if (!DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            throw new CborSerializerException($"The text string '{dateString}' could not be parsed as DateOnly.");
        }

        return result;
    }

    protected override string FormatToString(DateOnly value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture); // ISO 8601 format (yyyy-MM-dd)
    }
}
