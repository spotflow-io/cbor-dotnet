namespace Spotflow.Cbor.Converters;

internal class CborArrayConverter() : CborCollectionConverterFactoryBase(typeof(GenericConverter<,>))
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsArray;

    protected override Type GetElementType(Type typeToConvert)
    {
        return typeToConvert.GetElementType() ?? throw new InvalidOperationException($"Could not get element type of array type '{typeToConvert}'.");
    }

    private class GenericConverter<TCollection, TElement> : CborCollectionConverterBase<TCollection, TElement?[], TElement>
        where TCollection : IEnumerable<TElement?>
    {
        protected override TElement?[] CreateCollection(int? definitiveLenght) => definitiveLenght.HasValue ? new TElement?[definitiveLenght.Value] : [];

        protected override TElement?[] AddToCollection(TElement?[] collection, TElement? element, int elementIndex)
        {
            if (collection.Length <= elementIndex)
            {
                var newLength = Math.Max(elementIndex + 1, collection.Length * 2);

                Array.Resize(ref collection, newLength);
            }

            collection[elementIndex] = element;

            return collection;
        }
    }
}



