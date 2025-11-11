using System.Formats.Cbor;
using System.Globalization;
using System.Numerics;

namespace Spotflow.Cbor.Converters;

internal class CborBigIntegerConverter : CborConverter<BigInteger>
{
    public override bool HandleNull => false;

    public override BigInteger Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.UnsignedInteger)
        {
            // For unsigned values, use UInt64 for better range
            var uint64Value = reader.ReadUInt64();
            return uint64Value;
        }

        if (state is CborReaderState.NegativeInteger)
        {
            // For negative values, use Int64
            var int64Value = reader.ReadInt64();
            return int64Value;
        }

        var allowReadingFromString = options.NumberHandling.HasFlag(CborNumberHandling.AllowReadingFromString);

        if (allowReadingFromString && state is CborReaderState.TextString)
        {
            var stringValue = reader.ReadTextString();
            return BigInteger.Parse(stringValue, NumberFormatInfo.InvariantInfo);
        }

        // Handle bignum tags (tag 2 for unsigned, tag 3 for negative)
        if (tag is CborTag.UnsignedBigNum)
        {
            var bytes = reader.ReadByteString();
            // CBOR bignums are unsigned big-endian
            return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        }

        if (tag is CborTag.NegativeBigNum)
        {
            var bytes = reader.ReadByteString();
            // CBOR negative bignum: value = -1 - n
            var bigInt = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
            return -1 - bigInt;
        }

        ReadOnlySpan<CborReaderState> expectedStates = allowReadingFromString switch
        {
            true => [CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger, CborReaderState.ByteString, CborReaderState.TextString],
            false => [CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger, CborReaderState.ByteString],
        };

        throw UnexpectedDataType(expectedStates, state);
    }

    public override void Write(CborWriter writer, BigInteger value, CborSerializerOptions options)
    {
        if (options.NumberHandling.HasFlag(CborNumberHandling.WriteAsString))
        {
            writer.WriteTextString(value.ToString(NumberFormatInfo.InvariantInfo));
        }
        else
        {
            // Handle negative values
            if (value < 0)
            {
                // If value fits in Int64, write as CBOR negative integer
                if (value >= long.MinValue)
                {
                    writer.WriteInt64((long) value);
                }
                else
                {
                    // Negative bignum: tag 3, value is -1 - n
                    writer.WriteTag(CborTag.NegativeBigNum);
                    var n = -1 - value;
                    var bytesNeeded = n.GetByteCount(isUnsigned: true);
                    Span<byte> bytes = stackalloc byte[bytesNeeded];
                    if (!n.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: true))
                    {
                        throw new InvalidOperationException($"Failed to write BigInteger value {value} as bignum bytes.");
                    }
                    writer.WriteByteString(bytes);
                }
            }
            else
            {
                // Handle positive values (including zero)
                // If value fits in UInt64, write as CBOR unsigned integer
                if (value <= ulong.MaxValue)
                {
                    writer.WriteUInt64((ulong) value);
                }
                else
                {
                    // Positive bignum: tag 2
                    writer.WriteTag(CborTag.UnsignedBigNum);
                    var bytesNeeded = value.GetByteCount(isUnsigned: true);
                    Span<byte> bytes = stackalloc byte[bytesNeeded];
                    if (!value.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: true))
                    {
                        throw new InvalidOperationException($"Failed to write BigInteger value {value} as bignum bytes.");
                    }
                    writer.WriteByteString(bytes);
                }
            }
        }
    }
}
