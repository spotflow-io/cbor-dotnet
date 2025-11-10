using System.Collections.Frozen;

namespace Spotflow.Cbor.Converters;

internal class CborFrozenDictionaryConverter() : CborDictionaryConverterFactoryBase(typeof(GenericConverter<,>))
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericArguments().Length != 2)
        {
            return false;
        }
        var genericTypeDefinitionToConvert = typeToConvert.GetGenericTypeDefinition();
        return genericTypeDefinitionToConvert == typeof(FrozenDictionary<,>);
    }
    protected override Type GetKeyType(Type typeToConvert) => typeToConvert.GetGenericArguments()[0];

    protected override Type GetValueType(Type typeToConvert) => typeToConvert.GetGenericArguments()[1];

    private class GenericConverter<TKey, TValue> : CborDictionaryConverterBase<FrozenDictionary<TKey, TValue?>, Dictionary<TKey, TValue?>, TKey, TValue>
        where TKey : notnull
    {
        protected override Dictionary<TKey, TValue?> CreateDictionary() => [];
        protected override Dictionary<TKey, TValue?> AddToDictionary(Dictionary<TKey, TValue?> collection, TKey key, TValue? value)
        {
            collection[key] = value;
            return collection;
        }

        protected override FrozenDictionary<TKey, TValue?> ConvertToFinalDictionary(Dictionary<TKey, TValue?> dictionary)
        {
            return dictionary.ToFrozenDictionary();
        }
    }
}
