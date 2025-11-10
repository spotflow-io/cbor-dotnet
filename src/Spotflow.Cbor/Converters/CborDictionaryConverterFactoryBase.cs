namespace Spotflow.Cbor.Converters;

internal abstract class CborDictionaryConverterFactoryBase : CborConverterFactory
{
    private readonly Type _genericConverterTypeDefinition;
    private readonly int _numberOfGenericArguments;

    protected CborDictionaryConverterFactoryBase(Type genericConverterTypeDefinition)
    {
        if (!genericConverterTypeDefinition.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"'{genericConverterTypeDefinition}' is not a generic type definition.");
        }

        _numberOfGenericArguments = genericConverterTypeDefinition.GetGenericArguments().Length;

        if (_numberOfGenericArguments is not 3 and not 2)
        {
            throw new InvalidOperationException($"'{genericConverterTypeDefinition}' does not have exactly two or threee generic arguments.");
        }

        _genericConverterTypeDefinition = genericConverterTypeDefinition;
    }

    protected abstract Type GetKeyType(Type typeToConvert);

    protected abstract Type GetValueType(Type typeToConvert);

    public sealed override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        var keyType = GetKeyType(typeToConvert);
        var valueType = GetValueType(typeToConvert);

        Type converterType;

        if (_numberOfGenericArguments == 2)
        {
            converterType = _genericConverterTypeDefinition.MakeGenericType(keyType, valueType);
        }
        else
        {
            converterType = _genericConverterTypeDefinition.MakeGenericType(typeToConvert, keyType, valueType);
        }

        var converter = (CborConverter?) Activator.CreateInstance(converterType);

        if (converter is null)
        {
            throw new InvalidOperationException($"Could not create an instance of type '{converterType}'.");
        }

        return converter;

    }


}
