using System.Formats.Cbor;
using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal class CborTimeSpanConverter : CborConverter<TimeSpan>
{
    // TimeSpan constant format: [-][d'.']hh':'mm':'ss['.'fffffff]
    // Maximum length calculation:
    // - Sign: 1 char ('-')
    // - Days: max 10 digits (Int32.MaxValue days) + 1 char ('.')
    // - Hours: 2 digits
    // - Minutes: 2 digits + 1 char (':')
    // - Seconds: 2 digits + 2 chars (':' and '.')
    // - Fractional seconds: 7 digits
    // Total: 1 + 10 + 1 + 2 + 1 + 2 + 1 + 2 + 1 + 7 = 28 characters
    private const int _maxTimeSpanStringLength = 28;

    public override bool HandleNull => false;

    public override TimeSpan Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.TextString)
        {
            Span<char> buffer = stackalloc char[_maxTimeSpanStringLength];

            if (!reader.TryReadTextString(buffer, out var charsWritten))
            {
                // String is longer than expected, fall back to heap allocation
                var durationString = reader.ReadTextString();

                if (TimeSpan.TryParse(durationString, CultureInfo.InvariantCulture, out var timeSpan))
                {
                    return timeSpan;
                }

                throw new CborSerializerException($"The text string '{durationString}' could not be parsed as TimeSpan.");
            }

            var span = buffer[..charsWritten];

            if (TimeSpan.TryParse(span, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new CborSerializerException($"The text string '{new string(span)}' could not be parsed as TimeSpan.");
        }

        // Support reading from numeric values (treat as total seconds)
        if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger)
        {
            var seconds = reader.ReadInt64();
            return TimeSpan.FromSeconds(seconds);
        }

        if (state is CborReaderState.DoublePrecisionFloat or CborReaderState.SinglePrecisionFloat or CborReaderState.HalfPrecisionFloat)
        {
            var seconds = reader.ReadDouble();
            return TimeSpan.FromSeconds(seconds);
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

    public override void Write(CborWriter writer, TimeSpan value, CborSerializerOptions options)
    {
        // Use .NET's constant format: [-][d'.']hh':'mm':'ss['.'fffffff]
        Span<char> buffer = stackalloc char[_maxTimeSpanStringLength];

        if (!value.TryFormat(buffer, out var charsWritten, "c", CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException($"Failed to format TimeSpan value {value}.");
        }

        writer.WriteTextString(buffer[..charsWritten]);
    }
}
