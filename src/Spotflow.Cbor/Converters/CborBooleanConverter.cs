using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

public class CborBooleanConverter : CborConverter<bool>
{
    public override bool HandleNull => false;

    public override bool Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.Boolean)
        {
            return reader.ReadBoolean();
        }

        if (options.LaxBooleanParsing)
        {
            if (state is CborReaderState.UnsignedInteger)
            {
                return reader.ReadUInt64() != 0;
            }

            if (state is CborReaderState.NegativeInteger)
            {
                return reader.ReadCborNegativeIntegerRepresentation() != 0;
            }

            if (state is CborReaderState.TextString)
            {
                var value = reader.ReadTextString();

                if (bool.TryParse(value, out var result))
                {
                    return result;
                }

                throw new CborDataSerializationException($"Value '{value}' cannot be parsed as boolean.");
            }

            throw UnexpectedDataType([CborReaderState.Boolean, CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger, CborReaderState.TextString], state);
        }
        else
        {
            throw UnexpectedDataType([CborReaderState.Boolean], state);
        }


    }

    public override void Write(CborWriter writer, bool value, CborSerializerOptions options)
    {
        writer.WriteBoolean(value);
    }
}

