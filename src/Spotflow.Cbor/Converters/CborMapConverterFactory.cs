using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Spotflow.Cbor.Converters;

internal class CborMapConverterFactory : CborConverterFactory
{
    private static readonly ConcurrentDictionary<Type, CborConverter> _cache = new();

    private static readonly FrozenSet<Type> _supportedTypes = [
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>),
        typeof(Dictionary<,>)];

    public override bool CanConvert(Type typeToConvert)
    {
        return _supportedTypes.Contains(typeToConvert.IsGenericType ? typeToConvert.GetGenericTypeDefinition() : typeToConvert);
    }

    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return _cache.GetOrAdd(typeToConvert, CreateInstance);
    }

    private static CborConverter CreateInstance(Type type)
    {
        var genericArguments = type.GetGenericArguments();

        if (genericArguments.Length != 2)
        {
            throw new InvalidOperationException($"Type '{type}' has {genericArguments.Length} generic arguments, 2 expected.");
        }

        var keyType = genericArguments[0];
        var valueType = genericArguments[1];

        var converterType = typeof(CborMapConverter<,,>).MakeGenericType(type, keyType, valueType);

        var valueIsReferenceType = !valueType.IsValueType;
        var valueIsNullable = valueIsReferenceType || Nullable.GetUnderlyingType(valueType) is not null;

        var converter = (CborConverter?) Activator.CreateInstance(converterType, [valueIsNullable]);

        if (converter is null)
        {
            throw new InvalidOperationException($"Could not create an instance of type '{converterType}'.");
        }

        return converter;
    }
}
