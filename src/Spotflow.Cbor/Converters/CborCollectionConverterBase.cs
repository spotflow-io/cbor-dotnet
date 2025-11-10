using System.Formats.Cbor;
using System.Linq.Expressions;

namespace Spotflow.Cbor.Converters;

internal abstract class CborCollectionConverterBase<TCollection, TCollectionImpl, TElement> : CborConverter<TCollection>
    where TCollection : IEnumerable<TElement?>
{
    private readonly bool _elementIsNullable;
    private readonly Caster _caster = CreateCompiledCaster();

    private delegate TCollection Caster(TCollectionImpl collection);

    public CborCollectionConverterBase()
    {
        var elementType = typeof(TElement);
        var elementIsReferenceType = !elementType.IsValueType;
        _elementIsNullable = elementIsReferenceType || Nullable.GetUnderlyingType(elementType) is not null;
    }

    public override bool HandleNull => false;

    protected abstract TCollectionImpl CreateCollection(int? definitiveLenght);

    protected abstract TCollectionImpl AddToCollection(TCollectionImpl collection, TElement? element, int elementIndex);

    public override TCollection Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var initialDepth = reader.CurrentDepth;
        options.AssertMaxDepth(initialDepth);

        AssertReaderState(reader, CborReaderState.StartArray);

        var definitiveLength = reader.ReadStartArray();

        var collection = CreateCollection(definitiveLength);

        var elementIndex = 0;

        while (reader.PeekState() != CborReaderState.EndArray)
        {
            try
            {
                var elementConverter = CborTypeInfo.ResolveReadConverterForType<TElement>(reader, options);

                var element = CborSerializer.ResolveValue(_elementIsNullable, elementConverter, reader, options);

                collection = AddToCollection(collection, element, elementIndex);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentIndex(ex, initialDepth, elementIndex);
            }

            elementIndex++;
        }

        AssertReaderState(reader, CborReaderState.EndArray);

        reader.ReadEndArray();

        return _caster(collection);
    }

    public override void Write(CborWriter writer, TCollection? value, CborSerializerOptions options)
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

    private static Caster CreateCompiledCaster()
    {
        var collectionImplParameter = Expression.Parameter(typeof(TCollectionImpl), "collection");
        var body = Expression.Convert(collectionImplParameter, typeof(TCollection));
        var lambda = Expression.Lambda<Caster>(body, collectionImplParameter);
        return lambda.Compile();
    }

}
