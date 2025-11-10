namespace Spotflow.Cbor.Converters;

internal abstract class CborCollectionConverterFactoryBase : CborConverterFactory
{
    private readonly Type _genericConverterTypeDefinition;

    protected CborCollectionConverterFactoryBase(Type genericConverterTypeDefinition)
    {
        if (!genericConverterTypeDefinition.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"'{genericConverterTypeDefinition}' is not a generic type definition.");
        }

        if (genericConverterTypeDefinition.GetGenericArguments().Length != 2)
        {
            throw new InvalidOperationException($"'{genericConverterTypeDefinition}' does not have exactly two generic arguments.");
        }

        _genericConverterTypeDefinition = genericConverterTypeDefinition;
    }

    protected abstract Type GetElementType(Type typeToConvert);

    public sealed override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        var elementType = GetElementType(typeToConvert);

        var converterType = _genericConverterTypeDefinition.MakeGenericType(typeToConvert, elementType);

        var converter = (CborConverter?) Activator.CreateInstance(converterType);

        if (converter is null)
        {
            throw new InvalidOperationException($"Could not create an instance of type '{converterType}'.");
        }

        return converter;

    }


}
