using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborStringConverter : CborConverter<string>
{
    public override string? Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        AssertReaderState(reader, CborReaderState.TextString);

        return reader.ReadTextString();
    }

    public override void Write(CborWriter writer, string? value, CborSerializerOptions options)
    {
        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        writer.WriteTextString(value);

    }
}

