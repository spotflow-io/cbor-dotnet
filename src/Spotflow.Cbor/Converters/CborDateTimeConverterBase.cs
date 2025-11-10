using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal abstract class CborDateTimeConverterBase<T> : CborConverter<T> where T : struct
{
    public override bool HandleNull => false;

    protected abstract bool TryParseFromString(string dateString, out T result);
    protected abstract T ConvertFromUnixTimeSeconds(long seconds, long ticks = 0);
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

            if (!TryParseFromString(dateString, out var result))
            {
                throw new CborSerializerException($"The text string '{dateString}' could not be parsed as {typeof(T).Name}.");
            }

            return result;
        }

        // Handle tag 1: Unix time seconds (can be integer or floating-point)
        if (tag is CborTag.UnixTimeSeconds)
        {
            if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger)
            {
                var seconds = reader.ReadInt64();
                return ConvertFromUnixTimeSeconds(seconds);
            }
            else if (state is CborReaderState.DoublePrecisionFloat or CborReaderState.SinglePrecisionFloat or CborReaderState.HalfPrecisionFloat)
            {
                var seconds = reader.ReadDouble();
                var wholeSeconds = (long) seconds;
                var fractionalSeconds = seconds - wholeSeconds;
                var ticks = (long) (fractionalSeconds * TimeSpan.TicksPerSecond);

                return ConvertFromUnixTimeSeconds(wholeSeconds, ticks);
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

        // No tag: only accept text string (RFC 3339) or integer Unix time
        // We don't accept floating-point without a tag because it's ambiguous
        if (state is CborReaderState.TextString)
        {
            var dateString = reader.ReadTextString();

            if (!TryParseFromString(dateString, out var result))
            {
                throw new CborSerializerException($"The text string '{dateString}' could not be parsed as {typeof(T).Name}.");
            }

            return result;
        }
        else if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger)
        {
            var seconds = reader.ReadInt64();
            return ConvertFromUnixTimeSeconds(seconds);
        }

        throw UnexpectedDataType([
            CborReaderState.TextString,
            CborReaderState.UnsignedInteger,
            CborReaderState.NegativeInteger
        ], state);
    }

    public override void Write(CborWriter writer, T value, CborSerializerOptions options)
    {
        // Only write tag if explicitly enabled via options
        if (options.WriteDateTimeStringTag)
        {
            writer.WriteTag(CborTag.DateTimeString);
        }

        var dateString = FormatToString(value);
        writer.WriteTextString(dateString);
    }
}
