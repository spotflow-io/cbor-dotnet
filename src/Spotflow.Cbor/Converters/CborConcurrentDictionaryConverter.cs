using System.Collections.Concurrent;

namespace Spotflow.Cbor.Converters;

internal class CborConcurrentDictionaryConverter() : CborDictionaryConverterFactoryBase(typeof(GenericConverter<,>))
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericArguments().Length != 2)
        {
            return false;
        }

        var genericTypeDefinitionToConvert = typeToConvert.GetGenericTypeDefinition();

        return genericTypeDefinitionToConvert == typeof(ConcurrentDictionary<,>);

    }
    protected override Type GetKeyType(Type typeToConvert) => typeToConvert.GetGenericArguments()[0];
    protected override Type GetValueType(Type typeToConvert) => typeToConvert.GetGenericArguments()[1];

    private class GenericConverter<TKey, TValue> : CborDictionaryConverterBase<ConcurrentDictionary<TKey, TValue?>, TKey, TValue>
        where TKey : notnull
    {
        protected override ConcurrentDictionary<TKey, TValue?> CreateDictionary() => new();
        protected override ConcurrentDictionary<TKey, TValue?> AddToDictionary(ConcurrentDictionary<TKey, TValue?> collection, TKey key, TValue? value)
        {
            collection[key] = value;
            return collection;
        }
    }
}
