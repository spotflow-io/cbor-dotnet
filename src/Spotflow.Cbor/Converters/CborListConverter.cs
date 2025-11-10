using System.Collections.Frozen;

namespace Spotflow.Cbor.Converters;

internal class CborListConverter() : CborCollectionConverterFactoryBase(typeof(GenericConverter<,>))
{
    private static readonly FrozenSet<Type> _supportedTypes = [
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IReadOnlyCollection<>),
        typeof(IList<>),
        typeof(IReadOnlyList<>),
        typeof(List<>)];

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericArguments().Length != 1)
        {
            return false;
        }

        var genericTypeDefinitionToConvert = typeToConvert.GetGenericTypeDefinition();

        return _supportedTypes.Contains(genericTypeDefinitionToConvert);
    }

    protected override Type GetElementType(Type typeToConvert)
    {
        return typeToConvert.GetGenericArguments()[0];
    }

    private class GenericConverter<TCollection, TElement> : CborCollectionConverterBase<TCollection, List<TElement?>, TElement>
        where TCollection : IEnumerable<TElement?>
    {
        protected override List<TElement?> CreateCollection(int? definitiveLenght) => definitiveLenght.HasValue ? new(definitiveLenght.Value) : [];

        protected override List<TElement?> AddToCollection(List<TElement?> collection, TElement? element, int elementIndex)
        {
            collection.Add(element);
            return collection;
        }
    }
}


