using System.Formats.Cbor;
using System.Linq.Expressions;

namespace Spotflow.Cbor.Converters;

internal abstract class CborDictionaryConverterBase<TDictionary, TKey, TValue> : CborDictionaryConverterBase<TDictionary, TDictionary, TKey, TValue>
    where TDictionary : IEnumerable<KeyValuePair<TKey, TValue?>>
    where TKey : notnull;

internal abstract class CborDictionaryConverterBase<TDictionary, TDictionaryImpl, TKey, TValue> : CborConverter<TDictionary>
    where TDictionary : IEnumerable<KeyValuePair<TKey, TValue?>>
    where TKey : notnull
{
    private readonly bool _valueIsNullable;
    private Caster? _caster;

    private delegate TDictionary Caster(TDictionaryImpl collection);

    public CborDictionaryConverterBase()
    {
        var valueType = typeof(TValue);
        var valueIsReferenceType = !valueType.IsValueType;
        _valueIsNullable = valueIsReferenceType || Nullable.GetUnderlyingType(valueType) is not null;
    }

    public override bool HandleNull => false;

    protected abstract TDictionaryImpl CreateDictionary();

    protected abstract TDictionaryImpl AddToDictionary(TDictionaryImpl dictionary, TKey key, TValue? value);

    protected virtual TDictionary ConvertToFinalDictionary(TDictionaryImpl dictionary)
    {
        _caster ??= CreateCompiledCaster();
        return _caster(dictionary);
    }

    public override TDictionary Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var initialDepth = reader.CurrentDepth;

        options.AssertMaxDepth(initialDepth);

        AssertReaderState(reader, CborReaderState.StartMap);

        reader.ReadStartMap();

        var dictionary = CreateDictionary();

        while (reader.PeekState() != CborReaderState.EndMap)
        {
            TKey? key;

            var keyConverter = CborTypeInfo.ResolveReadConverterForType<TKey>(reader, options);

            if (reader.PeekState() is CborReaderState.Null && !keyConverter.HandleNull)
            {
                throw new InvalidOperationException("Dictionary key cannot be null.");
            }

            key = keyConverter.Read(reader, typeof(TKey), tag: null, options);

            if (key is null)
            {
                throw new CborSerializerException("Dictionary key cannot be null.");
            }

            try
            {
                var valueConverter = CborTypeInfo.ResolveReadConverterForType<TValue>(reader, options);

                var value = CborSerializer.ResolveValue(_valueIsNullable, valueConverter, reader, options);

                AddToDictionary(dictionary, key, value);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentKey(ex, initialDepth, key);
            }
        }

        AssertReaderState(reader, CborReaderState.EndMap);

        reader.ReadEndMap();

        return ConvertToFinalDictionary(dictionary);
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

    private static Caster CreateCompiledCaster()
    {
        var collectionImplParameter = Expression.Parameter(typeof(TDictionaryImpl), "dictionary");
        var body = Expression.Convert(collectionImplParameter, typeof(TDictionary));
        var lambda = Expression.Lambda<Caster>(body, collectionImplParameter);
        return lambda.Compile();
    }

}
