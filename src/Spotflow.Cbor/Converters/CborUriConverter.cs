using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborUriConverter : CborConverter<Uri>
{
    public override bool HandleNull => false;

    public override Uri? Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is not CborReaderState.TextString)
        {
            throw UnexpectedDataType(CborReaderState.TextString, state);
        }

        var uriString = reader.ReadTextString();

        if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
        {
            throw new CborSerializerException($"The text string '{uriString}' could not be parsed as URI.");
        }

        return uri;
    }

    public override void Write(CborWriter writer, Uri? value, CborSerializerOptions options)
    {
        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        writer.WriteTextString(value.ToString());
    }
}
