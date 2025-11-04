namespace Spotflow.Cbor.Converters;

public class CborStringEnumConverter(bool caseSensitive) : CborConverterFactory
{
    public CborStringEnumConverter() : this(caseSensitive: false)
    {
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: true, allowDeserializationFromString: true, caseSensitive: caseSensitive, options);
    }
}

public class CborStringEnumConverter<T>(bool caseSensitive) : CborConverterFactory where T : struct, Enum
{
    public CborStringEnumConverter() : this(caseSensitive: false)
    {
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: true, allowDeserializationFromString: true, caseSensitive: caseSensitive, options);
    }
}

