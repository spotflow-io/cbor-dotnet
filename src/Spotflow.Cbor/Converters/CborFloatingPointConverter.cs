using System.Formats.Cbor;
using System.Globalization;

namespace Spotflow.Cbor.Converters;

internal static class CborFloatingPointConverter
{
    public class Half : CborConverter<System.Half>
    {
        public override bool HandleNull => false;

        public override System.Half Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return ReadValue(
                reader,
                [CborReaderState.HalfPrecisionFloat],
                static reader => reader.ReadHalf(),
                System.Half.Parse,
                options);
        }

        public override void Write(CborWriter writer, System.Half value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (writer, value) => writer.WriteHalf(value), options);
        }
    }

    public class Single : CborConverter<float>
    {
        public override bool HandleNull => false;

        public override float Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return ReadValue(
                reader,
                [CborReaderState.SinglePrecisionFloat, CborReaderState.HalfPrecisionFloat],
                static reader => reader.ReadSingle(),
                float.Parse,
                options);
        }

        public override void Write(CborWriter writer, float value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (writer, value) => writer.WriteSingle(value), options);
        }
    }

    public class Double : CborConverter<double>
    {
        public override bool HandleNull => false;

        public override double Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return ReadValue(
                reader,
                [CborReaderState.DoublePrecisionFloat, CborReaderState.SinglePrecisionFloat, CborReaderState.HalfPrecisionFloat],
                static reader => reader.ReadDouble(),
                double.Parse,
                options);
        }

        public override void Write(CborWriter writer, double value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (writer, value) => writer.WriteDouble(value), options);
        }
    }

    private static T ReadValue<T>(
        CborReader reader,
        ReadOnlySpan<CborReaderState> expectedNumericStates,
        Func<CborReader, T> readNumeric,
#if NET9_0_OR_GREATER
        Func<ReadOnlySpan<char>, NumberFormatInfo, T> parse,
#else
        Func<string, NumberFormatInfo, T> parse,
#endif
        CborSerializerOptions options) where T : ISpanFormattable
    {
        var state = reader.PeekState();

        if (contains(expectedNumericStates, state))
        {
            return readNumeric(reader);
        }

        var allowReadingFromString = options.NumberHandling.HasFlag(CborNumberHandling.AllowReadingFromString);

        if (allowReadingFromString && state is CborReaderState.TextString)
        {
            var buffer = reader.ReadTextString();

            return parse(buffer, NumberFormatInfo.InvariantInfo);

            throw new CborContentException($"The text string '{buffer}' could not be parsed as a Half-precision floating point number.");
        }

        var expectedStates = allowReadingFromString switch
        {
            true => [.. expectedNumericStates, CborReaderState.TextString],
            false => expectedNumericStates,
        };

        throw CborConverter.UnexpectedDataType(expectedStates, state);

        static bool contains(ReadOnlySpan<CborReaderState> states, CborReaderState state)
        {
            foreach (var s in states)
            {
                if (s == state)
                {
                    return true;
                }
            }

            return false;
        }

    }

    private static void WriteValue<T>(CborWriter writer, T value, Action<CborWriter, T> write, CborSerializerOptions options) where T : IFormattable
    {
        if (options.NumberHandling.HasFlag(CborNumberHandling.WriteAsString))
        {
            var stringValue = value.ToString(null, formatProvider: NumberFormatInfo.InvariantInfo);
            writer.WriteTextString(stringValue);
        }
        else
        {
            write(writer, value);
        }
    }

}
