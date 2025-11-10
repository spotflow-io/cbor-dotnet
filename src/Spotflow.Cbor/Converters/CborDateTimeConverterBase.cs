using System.Formats.Cbor;
using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal abstract class CborDateTimeConverterBase<T>(bool supportsWritingDateTimeStringTag) : CborConverter<T> where T : struct
{
    public override bool HandleNull => false;

    protected abstract T ConvertFromDateTimeOffset(DateTimeOffset dateTimeOffset);
    protected abstract T ConvertFromStringWithoutTag(string dateString);
    protected abstract string FormatToString(T value);

    public override T Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        // Handle tag 0: RFC 3339 date/time string
        if (tag is CborTag.DateTimeString)
        {
            if (state is not CborReaderState.TextString)
            {
                throw new CborSerializerException($"Expected text string for {typeof(T).Name} with tag 0, got '{state}'.");
            }

            var dateString = reader.ReadTextString();

            if (!DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedDateTime))
            {
                throw new CborSerializerException($"The text string '{dateString}' could not be parsed as DateTimeOffset.");
            }

            return ConvertFromDateTimeOffset(parsedDateTime);
        }

        // Handle tag 1: Unix time seconds (can be integer or floating-point)
        if (tag is CborTag.UnixTimeSeconds)
        {
            return ReadUnixTime(reader, state);
        }

        // No tag: accept text string, integer Unix time, or floating-point Unix time
        if (state is CborReaderState.TextString)
        {
            var dateString = reader.ReadTextString();
            return ConvertFromStringWithoutTag(dateString);
        }
        else if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger
            or CborReaderState.DoublePrecisionFloat or CborReaderState.SinglePrecisionFloat or CborReaderState.HalfPrecisionFloat)
        {
            return ReadUnixTime(reader, state);
        }

        throw UnexpectedDataType([
            CborReaderState.TextString,
            CborReaderState.UnsignedInteger,
            CborReaderState.NegativeInteger,
            CborReaderState.DoublePrecisionFloat,
            CborReaderState.SinglePrecisionFloat,
            CborReaderState.HalfPrecisionFloat
        ], state);
    }

    private T ReadUnixTime(CborReader reader, CborReaderState state)
    {
        if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger)
        {
            var seconds = reader.ReadInt64();
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return ConvertFromDateTimeOffset(dateTimeOffset);
        }
        else if (state is CborReaderState.DoublePrecisionFloat or CborReaderState.SinglePrecisionFloat or CborReaderState.HalfPrecisionFloat)
        {
            var seconds = reader.ReadDouble();
            var wholeSeconds = (long) seconds;
            var fractionalSeconds = seconds - wholeSeconds;
            var ticks = (long) (fractionalSeconds * TimeSpan.TicksPerSecond);

            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(wholeSeconds).AddTicks(ticks);
            return ConvertFromDateTimeOffset(dateTimeOffset);
        }
        else
        {
            throw UnexpectedDataType([
                CborReaderState.UnsignedInteger,
                CborReaderState.NegativeInteger,
                CborReaderState.DoublePrecisionFloat,
                CborReaderState.SinglePrecisionFloat,
                CborReaderState.HalfPrecisionFloat
            ], state);
        }
    }

    public override void Write(CborWriter writer, T value, CborSerializerOptions options)
    {
        // Only write tag if explicitly enabled via options AND this converter supports it
        if (options.WriteDateTimeStringTag && supportsWritingDateTimeStringTag)
        {
            writer.WriteTag(CborTag.DateTimeString);
        }

        var dateString = FormatToString(value);
        writer.WriteTextString(dateString);
    }
}
