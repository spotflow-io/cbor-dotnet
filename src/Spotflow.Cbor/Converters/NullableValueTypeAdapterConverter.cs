using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class NullableValueTypeAdapterConverter<T>(CborConverter<T> inner) : CborConverter<T?> where T : struct
{
    public override bool HandleNull => false;

    public override T? Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        return inner.Read(reader, typeof(T), options);
    }

    public override void Write(CborWriter writer, T? value, CborSerializerOptions options)
    {
        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        inner.Write(writer, value.Value, options);
    }

}

