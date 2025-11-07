using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborMapConverter<TDictionary, TKey, TValue>(bool valueIsNullable) : CborConverter<TDictionary>
    where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{

    public override bool HandleNull => false;

    public override TDictionary Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        var initialDepth = reader.CurrentDepth;

        options.AssertMaxDepth(initialDepth);

        AssertReaderState(reader, CborReaderState.StartMap);

        reader.ReadStartMap();

        var dictionary = new Dictionary<TKey, TValue?>();

        while (reader.PeekState() != CborReaderState.EndMap)
        {
            TKey? key;

            var keyConverter = CborTypeInfo.ResolveReadConverterForType<TKey>(reader, options);

            if (reader.PeekState() is CborReaderState.Null && !keyConverter.HandleNull)
            {
                throw new InvalidOperationException("Dictionary key cannot be null.");
            }

            key = keyConverter.Read(reader, typeof(TKey), options);

            if (key is null)
            {
                throw new CborSerializerException("Dictionary key cannot be null.");
            }

            try
            {
                var valueConverter = CborTypeInfo.ResolveReadConverterForType<TValue>(reader, options);

                var value = CborSerializer.ResolveValue(valueIsNullable, valueConverter, reader, options);

                dictionary.Add(key, value);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentKey(ex, initialDepth, key);
            }
        }

        AssertReaderState(reader, CborReaderState.EndMap);

        reader.ReadEndMap();

        return (TDictionary) (IEnumerable<KeyValuePair<TKey, TValue>>) dictionary;
    }

    public override void Write(CborWriter writer, TDictionary? value, CborSerializerOptions options)
    {
        var initialDepth = writer.CurrentDepth;

        options.AssertMaxDepth(initialDepth);

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
            try
            {
                keyConverter.Write(writer, kvp.Key, options);
                valueConverter.Write(writer, kvp.Value, options);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentKey(ex, initialDepth, kvp.Key);
            }
        }

        writer.WriteEndMap();
    }

    private static Exception EnrichWithParentKey(Exception ex, int objectDepth, TKey key)
    {
        string name;

        if (key is string stringKey)
        {
            name = $"#{objectDepth}: [\"{stringKey}\"]";
        }
        else
        {
            name = $"#{objectDepth}: [{key}]";
        }

        return CborSerializerException.WrapWithParentScope(ex, name);
    }
}
