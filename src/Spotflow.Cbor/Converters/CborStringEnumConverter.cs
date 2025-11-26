namespace Spotflow.Cbor.Converters;

public class CborStringEnumConverter(bool caseSensitive = false, bool serializeAsInteger = false) : CborConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: !serializeAsInteger, allowDeserializationFromString: true, caseSensitive: caseSensitive, options);
    }
}

public class CborStringEnumConverter<T>(bool caseSensitive = false, bool serializeAsInteger = false) : CborConverterFactory where T : struct, Enum
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: !serializeAsInteger, allowDeserializationFromString: true, caseSensitive: caseSensitive, options);
    }
}

