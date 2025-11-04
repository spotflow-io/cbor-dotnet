namespace Spotflow.Cbor.Converters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Enum)]
public class CborConverterAttribute<TConverter> : Attribute where TConverter : CborConverter;
