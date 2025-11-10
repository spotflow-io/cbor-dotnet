using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborStringConverter : CborConverter<string>
{
    public override string? Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        AssertReaderState(reader, CborReaderState.TextString, CborReaderState.StartIndefiniteLengthTextString);

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

