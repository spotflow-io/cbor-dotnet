using System.Collections.Frozen;

namespace Spotflow.Cbor.Converters;

internal class CborDictionaryConverter() : CborDictionaryConverterFactoryBase(typeof(GenericConverter<,,>))
{
    private static readonly FrozenSet<Type> _supportedTypes = [
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>),
        typeof(Dictionary<,>)];

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericArguments().Length != 2)
        {
            return false;
        }

        var genericTypeDefinitionToConvert = typeToConvert.GetGenericTypeDefinition();

        return _supportedTypes.Contains(genericTypeDefinitionToConvert);
    }

    protected override Type GetKeyType(Type typeToConvert) => typeToConvert.GetGenericArguments()[0];

    protected override Type GetValueType(Type typeToConvert) => typeToConvert.GetGenericArguments()[1];

    private class GenericConverter<TDictionary, TKey, TValue> : CborDictionaryConverterBase<TDictionary, Dictionary<TKey, TValue?>, TKey, TValue>
        where TDictionary : IEnumerable<KeyValuePair<TKey, TValue?>>
        where TKey : notnull
    {
        protected override Dictionary<TKey, TValue?> CreateDictionary() => [];

        protected override Dictionary<TKey, TValue?> AddToDictionary(Dictionary<TKey, TValue?> collection, TKey key, TValue? value)
        {
            collection[key] = value;
            return collection;
        }
    }
}

