using System.Formats.Cbor;
using System.Globalization;
using System.Numerics;

namespace Spotflow.Cbor.Converters;

internal static class CborIntegerConverter
{
    private const int _maxStringLen = 20; // Max length of ulong and long as string
    private const int _maxInt128StringLen = 40; // Max length of Int128/UInt128 as string

    public class Byte : CborConverter<byte>
    {
        public override bool HandleNull => false;

        public override byte Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return (byte) ReadUInt64Value(reader, byte.MaxValue, options);
        }

        public override void Write(CborWriter writer, byte value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteUInt32(v), options);
        }
    }

    public class SByte : CborConverter<sbyte>
    {
        public override bool HandleNull => false;

        public override sbyte Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return (sbyte) ReadInt64Value(reader, sbyte.MinValue, sbyte.MaxValue, options);
        }

        public override void Write(CborWriter writer, sbyte value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteInt32(v), options);
        }
    }

    public class Int16 : CborConverter<short>
    {
        public override bool HandleNull => false;

        public override short Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return (short) ReadInt64Value(reader, short.MinValue, short.MaxValue, options);
        }

        public override void Write(CborWriter writer, short value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteInt32(v), options);
        }
    }

    public class UInt16 : CborConverter<ushort>
    {
        public override bool HandleNull => false;

        public override ushort Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return (ushort) ReadUInt64Value(reader, ushort.MaxValue, options);
        }

        public override void Write(CborWriter writer, ushort value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteUInt32(v), options);
        }
    }

    public class Int32 : CborConverter<int>
    {
        public override bool HandleNull => false;

        public override int Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            var int64Value = ReadInt64Value(reader, int.MinValue, int.MaxValue, options);

            return (int) int64Value;
        }

        public override void Write(CborWriter writer, int value, CborSerializerOptions options)
        {
            if (options.NumberHandling.HasFlag(CborNumberHandling.WriteAsString))
            {
                writer.WriteTextString(value.ToString(NumberFormatInfo.InvariantInfo));
            }
            else
            {
                writer.WriteInt32(value);
            }
        }
    }

    public class UInt32 : CborConverter<uint>
    {
        public override bool HandleNull => false;

        public override uint Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return (uint) ReadUInt64Value(reader, uint.MaxValue, options);
        }

        public override void Write(CborWriter writer, uint value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteUInt32(v), options);
        }
    }

    public class Int64 : CborConverter<long>
    {
        public override bool HandleNull => false;

        public override long Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return ReadInt64Value(reader, long.MinValue, long.MaxValue, options);
        }

        public override void Write(CborWriter writer, long value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteInt64(v), options);
        }
    }

    public class UInt64 : CborConverter<ulong>
    {
        public override bool HandleNull => false;

        public override ulong Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            return ReadUInt64Value(reader, ulong.MaxValue, options);
        }

        public override void Write(CborWriter writer, ulong value, CborSerializerOptions options)
        {
            WriteValue(writer, value, static (w, v) => w.WriteUInt64(v), options);
        }
    }

    public class Int128 : CborConverter<System.Int128>
    {
        private static readonly CborBigIntegerConverter _bigIntegerConverter = new();

        public override bool HandleNull => false;

        public override System.Int128 Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            var bigInt = _bigIntegerConverter.Read(reader, typeof(BigInteger), tag, options);

            return (System.Int128) bigInt;
        }

        public override void Write(CborWriter writer, System.Int128 value, CborSerializerOptions options)
        {
            _bigIntegerConverter.Write(writer, (BigInteger) value, options);
        }
    }

    public class UInt128 : CborConverter<System.UInt128>
    {
        public override bool HandleNull => false;

        private static readonly CborBigIntegerConverter _bigIntegerConverter = new();

        public override System.UInt128 Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            var bigInt = _bigIntegerConverter.Read(reader, typeof(BigInteger), tag, options);

            return (System.UInt128) bigInt;
        }

        public override void Write(CborWriter writer, System.UInt128 value, CborSerializerOptions options)
        {
            _bigIntegerConverter.Write(writer, (BigInteger) value, options);
        }
    }

    private static ulong ReadUInt64Value(CborReader reader, ulong maxValue, CborSerializerOptions options)
    {
        var value = readCore(reader, options);

        if (value > maxValue)
        {
            throw new CborContentException($"Value {value} is out of range <0, {maxValue}>.");
        }

        return value;

        static ulong readCore(CborReader reader, CborSerializerOptions options)
        {
            var state = reader.PeekState();

            if (state is CborReaderState.UnsignedInteger)
            {
                return reader.ReadUInt64();
            }

            var allowReadingFromString = options.NumberHandling.HasFlag(CborNumberHandling.AllowReadingFromString);

            if (allowReadingFromString && state is CborReaderState.TextString)
            {
                Span<char> buffer = stackalloc char[_maxStringLen]; // Max length of ulong as string

                if (!reader.TryReadTextString(buffer, out var charsRead))
                {
                    throw new CborContentException("Failed to read string representation of integer - too long.");
                }

                return ulong.Parse(buffer[0..charsRead], NumberFormatInfo.InvariantInfo);
            }

            ReadOnlySpan<CborReaderState> expectedStates = allowReadingFromString switch
            {
                true => [CborReaderState.UnsignedInteger, CborReaderState.TextString],
                false => [CborReaderState.UnsignedInteger],
            };

            throw CborConverter.UnexpectedDataType(expectedStates, state);
        }

    }

    private static long ReadInt64Value(CborReader reader, long minValue, long maxValue, CborSerializerOptions options)
    {
        var value = readCore(reader, options);

        if (value < minValue || value > maxValue)
        {
            throw new CborContentException($"Value {value} is out of range <{minValue}, {maxValue}>.");
        }

        return value;

        static long readCore(CborReader reader, CborSerializerOptions options)
        {
            var state = reader.PeekState();

            if (state is CborReaderState.UnsignedInteger or CborReaderState.NegativeInteger)
            {
                return reader.ReadInt64();
            }

            var allowReadingFromString = options.NumberHandling.HasFlag(CborNumberHandling.AllowReadingFromString);

            if (allowReadingFromString && state is CborReaderState.TextString)
            {
                Span<char> buffer = stackalloc char[_maxStringLen];

                if (!reader.TryReadTextString(buffer, out var charsRead))
                {
                    throw new CborContentException("Failed to read string representation of integer - too long.");
                }

                return long.Parse(buffer[0..charsRead], NumberFormatInfo.InvariantInfo);
            }

            ReadOnlySpan<CborReaderState> expectedStates = allowReadingFromString switch
            {
                true => [CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger, CborReaderState.TextString],
                false => [CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger],
            };

            throw CborConverter.UnexpectedDataType(expectedStates, state);
        }
    }

    private static void WriteValue<T>(CborWriter writer, T value, Action<CborWriter, T> writeAsNumber, CborSerializerOptions options) where T : struct, ISpanFormattable
    {
        if (options.NumberHandling.HasFlag(CborNumberHandling.WriteAsString))
        {
            Span<char> buffer = stackalloc char[_maxStringLen];

            if (!value.TryFormat(buffer, out var charsWritten, format: null, provider: NumberFormatInfo.InvariantInfo))
            {
                throw new InvalidOperationException($"Failed to format value {value} as string.");
            }

            writer.WriteTextString(buffer[..charsWritten]);
        }
        else
        {
            writeAsNumber(writer, value);
        }
    }



}
