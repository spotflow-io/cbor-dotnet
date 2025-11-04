namespace Spotflow.Cbor.Converters;

internal struct CborListBuilder<TList, TElement>
{
    private List<TElement?>? _underlyingList;
    private TElement?[]? _underlyingArray;
    private int _underlyingArrayIndex = 0;

    public static Func<int?, CborListBuilder<TList, TElement>> PrepareListBuilderFactory()
    {
        var isArray = typeof(TList).IsArray;

        return (int? definitiveLength) => new CborListBuilder<TList, TElement>(isArray, definitiveLength);
    }

    public CborListBuilder(bool isArray, int? definitiveLength)
    {
        var lenght = definitiveLength ?? 4;

        ArgumentOutOfRangeException.ThrowIfNegative(lenght, nameof(definitiveLength));

        if (isArray)
        {
            _underlyingArray = new TElement?[lenght];
        }
        else
        {
            _underlyingList = new List<TElement?>(lenght);
        }
    }

    public void Add(TElement? element)
    {
        if (_underlyingList is not null)
        {
            _underlyingList.Add(element);
            return;
        }

        if (_underlyingArray is not null)
        {
            if (_underlyingArrayIndex >= _underlyingArray.Length)
            {
                // Resize array
                Array.Resize(ref _underlyingArray, _underlyingArray.Length * 2);
            }
            _underlyingArray[_underlyingArrayIndex++] = element;
            return;
        }

        throw new InvalidOperationException("Builder is not initialized.");

    }

    public TList Build()
    {
        if (_underlyingArray is not null)
        {
            if (_underlyingArrayIndex < _underlyingArray.Length)
            {
                Array.Resize(ref _underlyingArray, _underlyingArrayIndex);
            }

            var result = (TList) (object) _underlyingArray;
            _underlyingArray = null;
            return result;
        }

        if (_underlyingList is not null)
        {
            var result = (TList) (object) _underlyingList;
            _underlyingList = null;
            return result;
        }

        throw new InvalidOperationException("Builder is not initialized.");
    }
}
