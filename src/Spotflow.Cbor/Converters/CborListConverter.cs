using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborListConverter<TList, TElement>(
    bool elementIsNullable,
    Func<int?, CborListBuilder<TList, TElement>> listBuilderFactory
    ) : CborConverter<TList> where TList : IEnumerable<TElement>
{
    public override TList Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        AssertReaderState(reader, CborReaderState.StartArray);

        var definitiveLength = reader.ReadStartArray();

        var listBuilder = listBuilderFactory(definitiveLength);

        while (reader.PeekState() != CborReaderState.EndArray)
        {
            var elementConverter = CborTypeInfo.ResolveReadConverterForType<TElement>(reader, options);

            var element = CborSerializer.ResolveValue(elementIsNullable, elementConverter, reader, options);

            listBuilder.Add(element);
        }

        AssertReaderState(reader, CborReaderState.EndArray);

        reader.ReadEndArray();

        return listBuilder.Build();
    }

    public override void Write(CborWriter writer, TList? value, CborSerializerOptions options)
    {
        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        int? definiteLength = null;

        if (value.TryGetNonEnumeratedCount(out var count))
        {
            definiteLength = count;
        }

        writer.WriteStartArray(definiteLength);

        var elementConverter = CborTypeInfo.ResolveWriteConverterForType<TElement>(options);

        foreach (var element in value)
        {
            elementConverter.Write(writer, element, options);
        }

        writer.WriteEndArray();
    }
}
