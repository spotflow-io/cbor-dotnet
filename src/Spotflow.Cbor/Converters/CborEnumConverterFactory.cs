namespace Spotflow.Cbor.Converters;

internal static class CborEnumConverterFactory
{
    public static CborConverter CreateConverter(Type typeToConvert, bool serializeAsString, bool allowDeserializationFromString, bool caseSensitive, CborSerializerOptions options)
    {
        typeToConvert = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        if (!typeToConvert.IsEnum)
        {
            throw new InvalidOperationException($"Type '{typeToConvert}' is not an enum.");
        }

        var underlyingType = Enum.GetUnderlyingType(typeToConvert);

        var converterType = typeof(CborEnumConverter<,>).MakeGenericType(typeToConvert, underlyingType);

        var converter = (CborConverter?) Activator.CreateInstance(converterType, [serializeAsString, allowDeserializationFromString, caseSensitive]);

        if (converter is null)
        {
            throw new InvalidOperationException($"Failed to create instance of converter '{converterType}'.");
        }

        return converter;
    }
}

