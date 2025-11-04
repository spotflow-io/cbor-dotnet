using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;

namespace Spotflow.Cbor.Converters;

internal class CborListConverterFactory : CborConverterFactory
{
    private static readonly ConcurrentDictionary<Type, CborConverter> _cache = new();

    private static readonly FrozenSet<Type> _supportedTypes = [
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IReadOnlyCollection<>),
        typeof(IList<>),
        typeof(IReadOnlyList<>),
        typeof(List<>)];

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsArray)
        {
            return true;
        }

        return _supportedTypes.Contains(typeToConvert.IsGenericType ? typeToConvert.GetGenericTypeDefinition() : typeToConvert);
    }

    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return _cache.GetOrAdd(typeToConvert, CreateInstance);
    }

    private static CborConverter CreateInstance(Type type)
    {
        Type elementType;

        if (type.IsArray)
        {
            elementType = type.GetElementType() ?? throw new InvalidOperationException($"Could not get element type of array type '{type}'.");
        }
        else
        {
            var genericArguments = type.GetGenericArguments();


            if (genericArguments.Length != 1)
            {
                throw new InvalidOperationException($"Type '{type}' has {genericArguments.Length} generic arguments, 1 expected.");
            }

            elementType = genericArguments[0];
        }

        var converterType = typeof(CborListConverter<,>).MakeGenericType(type, elementType);

        var elementIsReferenceType = !elementType.IsValueType;
        var elementIsNullable = elementIsReferenceType || Nullable.GetUnderlyingType(elementType) is not null;

        var listBuilderFactory = PrepareListBuilderFactory(type, elementType);

        var converter = (CborConverter?) Activator.CreateInstance(converterType, [elementIsNullable, listBuilderFactory]);

        if (converter is null)
        {
            throw new InvalidOperationException($"Could not create an instance of type '{converterType}'.");
        }

        return converter;
    }

    private static object PrepareListBuilderFactory(Type listType, Type elementType)
    {
        var builderType = typeof(CborListBuilder<,>).MakeGenericType(listType, elementType);

        var prepareFactoryMethodName = nameof(CborListBuilder<object, object>.PrepareListBuilderFactory);
        var prepareFactoryMethod = builderType.GetMethod(prepareFactoryMethodName, BindingFlags.Public | BindingFlags.Static);

        if (prepareFactoryMethod is null)
        {
            throw new InvalidOperationException("Prepare method not found.");
        }

        var factory = prepareFactoryMethod.Invoke(null, null);

        if (factory is null)
        {
            throw new InvalidOperationException("Could not create list builder factory.");
        }

        return factory;
    }
}
