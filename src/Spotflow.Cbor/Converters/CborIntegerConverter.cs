using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal static class CborIntegerConverter
{
    public class Int32 : CborConverter<int>
    {
        public override bool HandleNull => false;

        public override int Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger);

            return reader.ReadInt32();
        }

        public override void Write(CborWriter writer, int value, CborSerializerOptions options)
        {
            writer.WriteInt32(value);
        }
    }

    public class Int64 : CborConverter<long>
    {
        public override bool HandleNull => false;

        public override long Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger);

            return reader.ReadInt64();
        }

        public override void Write(CborWriter writer, long value, CborSerializerOptions options)
        {
            writer.WriteInt64(value);
        }
    }
}
