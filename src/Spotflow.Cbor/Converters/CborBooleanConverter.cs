using System.Collections.Concurrent;
using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

public class CborBooleanConverter : CborConverter<bool>
{
    public override bool HandleNull => false;

    private static readonly ConcurrentDictionary<CborBooleanHandling, CborReaderState[]> _allowedStates = [];

    public override bool Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.Boolean)
        {
            return reader.ReadBoolean();
        }

        if (options.BooleanHandling.HasFlag(CborBooleanHandling.AllowReadingFromInteger))
        {
            if (state is CborReaderState.UnsignedInteger)
            {
                return reader.ReadUInt64() != 0;
            }
            else if (state is CborReaderState.NegativeInteger)
            {
                // neg_rep = -1 - actual_val
                // ...
                // actual_val = -3 => neg_rep = 2
                // actual_val = -2 => neg_rep = 1
                // actual_val = -1 => neg_rep = 0

                // => Actual value cannot be 0 as per CBOR spec, so any negative integer represents true

                reader.SkipValue();

                return true;
            }
        }

        if (options.BooleanHandling.HasFlag(CborBooleanHandling.AllowReadingFromString))
        {
            if (state is CborReaderState.TextString)
            {
                var value = reader.ReadTextString();

                if (bool.TryParse(value, out var result))
                {
                    return result;
                }

                throw new CborSerializerException($"Value '{value}' cannot be parsed as boolean.");
            }
        }

        var allowedStates = _allowedStates.GetOrAdd(options.BooleanHandling, GetAllowedStates);

        throw UnexpectedDataType(allowedStates, state);
    }

    public override void Write(CborWriter writer, bool value, CborSerializerOptions options)
    {
        writer.WriteBoolean(value);
    }

    private static CborReaderState[] GetAllowedStates(CborBooleanHandling handling)
    {
        var states = new List<CborReaderState> { CborReaderState.Boolean };

        if (handling.HasFlag(CborBooleanHandling.AllowReadingFromInteger))
        {
            states.Add(CborReaderState.UnsignedInteger);
            states.Add(CborReaderState.NegativeInteger);
        }

        if (handling.HasFlag(CborBooleanHandling.AllowReadingFromString))
        {
            states.Add(CborReaderState.TextString);
        }

        return [.. states];
    }
}

