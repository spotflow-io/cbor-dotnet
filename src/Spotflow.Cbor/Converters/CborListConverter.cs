using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborListConverter<TList, TElement>(
    bool elementIsNullable,
    Func<int?, CborListBuilder<TList, TElement>> listBuilderFactory
    ) : CborConverter<TList> where TList : IEnumerable<TElement>
{
    public override TList Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var initialDepth = reader.CurrentDepth;
        options.AssertMaxDepth(initialDepth);

        AssertReaderState(reader, CborReaderState.StartArray);

        var definitiveLength = reader.ReadStartArray();

        var listBuilder = listBuilderFactory(definitiveLength);

        var elementIndex = 0;

        while (reader.PeekState() != CborReaderState.EndArray)
        {
            try
            {
                var elementConverter = CborTypeInfo.ResolveReadConverterForType<TElement>(reader, options);

                var element = CborSerializer.ResolveValue(elementIsNullable, elementConverter, reader, options);

                listBuilder.Add(element);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentIndex(ex, initialDepth, elementIndex);
            }

            elementIndex++;
        }

        AssertReaderState(reader, CborReaderState.EndArray);

        reader.ReadEndArray();

        return listBuilder.Build();
    }

    public override void Write(CborWriter writer, TList? value, CborSerializerOptions options)
    {

        var initialDepth = writer.CurrentDepth;

        options.AssertMaxDepth(initialDepth);

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

        var elementIndex = 0;
        foreach (var element in value)
        {
            try
            {
                elementConverter.Write(writer, element, options);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentIndex(ex, initialDepth, elementIndex);
            }

            elementIndex++;
        }

        writer.WriteEndArray();
    }

    private static Exception EnrichWithParentIndex(Exception ex, int objectDepth, int index)
    {
        var name = $"#{objectDepth}: [{index}]";

        return CborSerializerException.WrapWithParentScope(ex, name);
    }
}
