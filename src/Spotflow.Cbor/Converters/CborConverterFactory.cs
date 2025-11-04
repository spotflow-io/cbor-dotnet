namespace Spotflow.Cbor.Converters;

public abstract class CborConverterFactory : CborConverter
{
    public abstract CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options);
}
