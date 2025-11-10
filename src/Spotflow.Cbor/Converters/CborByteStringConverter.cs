using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal static class CborByteStringConverter
{
    public class Array : CborConverter<byte[]>
    {
        public override byte[] Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.ByteString, CborReaderState.StartIndefiniteLengthByteString);

            return reader.ReadByteString();
        }

        public override void Write(CborWriter writer, byte[]? value, CborSerializerOptions options)
        {
            if (value is null)
            {
                throw CannotSerializeNullValue();
            }

            writer.WriteByteString(value);
        }
    }

    public class Memory : CborConverter<Memory<byte>>
    {
        public override Memory<byte> Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.ByteString, CborReaderState.StartIndefiniteLengthByteString);
            return reader.ReadByteString();
        }

        public override void Write(CborWriter writer, Memory<byte> value, CborSerializerOptions options)
        {
            writer.WriteByteString(value.Span);
        }
    }

    public class ReadOnlyMemory : CborConverter<ReadOnlyMemory<byte>>
    {
        public override ReadOnlyMemory<byte> Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.ByteString, CborReaderState.StartIndefiniteLengthByteString);
            return reader.ReadByteString();
        }

        public override void Write(CborWriter writer, ReadOnlyMemory<byte> value, CborSerializerOptions options)
        {
            writer.WriteByteString(value.Span);
        }
    }
}

