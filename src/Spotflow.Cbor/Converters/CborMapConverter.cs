using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborMapConverter<TDictionary, TKey, TValue>(bool valueIsNullable) : CborConverter<TDictionary>
    where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{

    public override bool HandleNull => false;

    public override TDictionary Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        AssertReaderState(reader, CborReaderState.StartMap);

        reader.ReadStartMap();

        var dictionary = new Dictionary<TKey, TValue?>();

        while (reader.PeekState() != CborReaderState.EndMap)
        {
            TKey? key;

            var keyConverter = CborTypeInfo.ResolveReadConverterForType<TKey>(reader, options);

            if (reader.PeekState() != CborReaderState.Null || keyConverter.HandleNull)
            {
                key = keyConverter.Read(reader, typeof(TKey), options);

                if (key is null)
                {
                    throw new CborDataSerializationException("Dictionary key cannot be null.");
                }
            }
            else
            {
                throw new InvalidOperationException("Dictionary key cannot be null.");
            }

            var valueConverter = CborTypeInfo.ResolveReadConverterForType<TValue>(reader, options);

            var value = CborSerializer.ResolveValue(valueIsNullable, valueConverter, reader, options);

            dictionary.Add(key, value);


        }

        AssertReaderState(reader, CborReaderState.EndMap);

        reader.ReadEndMap();

        return (TDictionary) (IEnumerable<KeyValuePair<TKey, TValue>>) dictionary;
    }

    public override void Write(CborWriter writer, TDictionary? value, CborSerializerOptions options)
    {
        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        int? definiteLength = null;

        if (value.TryGetNonEnumeratedCount(out var count))
        {
            definiteLength = count;
        }

        writer.WriteStartMap(definiteLength);

        var keyConverter = CborTypeInfo.ResolveWriteConverterForType<TKey>(options);
        var valueConverter = CborTypeInfo.ResolveWriteConverterForType<TValue>(options);

        foreach (var kvp in value)
        {
            keyConverter.Write(writer, kvp.Key, options);
            valueConverter.Write(writer, kvp.Value, options);
        }

        writer.WriteEndMap();
    }
}
