using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal static class CborFloatingPointConverter
{
    public class Half : CborConverter<System.Half>
    {
        public override bool HandleNull => false;

        public override System.Half Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.HalfPrecisionFloat);
            return reader.ReadHalf();
        }

        public override void Write(CborWriter writer, System.Half value, CborSerializerOptions options)
        {
            writer.WriteHalf(value);
        }
    }

    public class Single : CborConverter<float>
    {
        public override bool HandleNull => false;

        public override float Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.SinglePrecisionFloat);
            return reader.ReadSingle();
        }

        public override void Write(CborWriter writer, float value, CborSerializerOptions options)
        {
            writer.WriteSingle(value);
        }
    }

    public class Double : CborConverter<double>
    {
        public override bool HandleNull => false;

        public override double Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
        {
            AssertReaderState(reader, CborReaderState.DoublePrecisionFloat);
            return reader.ReadDouble();
        }

        public override void Write(CborWriter writer, double value, CborSerializerOptions options)
        {
            writer.WriteDouble(value);
        }
    }
}
