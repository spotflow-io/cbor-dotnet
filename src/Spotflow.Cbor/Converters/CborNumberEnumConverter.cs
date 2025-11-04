namespace Spotflow.Cbor.Converters;

public class CborNumberEnumConverter(bool caseSensitive) : CborConverterFactory
{
    public CborNumberEnumConverter() : this(caseSensitive: false)
    {
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: false, allowDeserializationFromString: false, caseSensitive: caseSensitive, options);
    }
}


public class CborNumberEnumConverter<T>(bool caseSensitive) : CborConverterFactory where T : struct, Enum
{
    public CborNumberEnumConverter() : this(caseSensitive: false)
    {
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);
    public override CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return CborEnumConverterFactory.CreateConverter(typeToConvert, serializeAsString: false, allowDeserializationFromString: false, caseSensitive: caseSensitive, options);
    }
}
