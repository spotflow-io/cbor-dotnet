using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

public abstract class CborConverter
{
    public abstract bool CanConvert(Type typeToConvert);

    protected static CborDataSerializationException UnexpectedDataType(IReadOnlyList<CborReaderState> expected, CborReaderState actual)
    {
        var expectedFormatted = expected.Count switch
        {
            0 => string.Empty,
            1 => $"'{expected[0]}'",
            2 => $"'{expected[0]}' or '{expected[1]}'",
            _ => $"{string.Join(", ", expected.Take(expected.Count - 1).Select(e => $"'{e}'"))} or '{expected[^1]}'"
        };

        return new($"Unexpected CBOR data type. Expected {expectedFormatted}, got '{actual}'.");
    }

    protected static CborDataSerializationException UnexpectedDataType(CborReaderState expected, CborReaderState actual)
    {
        return new($"Unexpected CBOR data type. Expected '{expected}', got '{actual}'.");
    }

    protected static void AssertReaderState(CborReader reader, params ReadOnlySpan<CborReaderState> expectedStates) => AssertReaderState(reader, out _, expectedStates);

    protected static void AssertReaderState(CborReader reader, out CborReaderState state, params ReadOnlySpan<CborReaderState> expectedStates)
    {
        var actualState = reader.PeekState();

        foreach (var expectedState in expectedStates)
        {
            if (expectedState == actualState)
            {
                state = actualState;
                return;
            }
        }

        throw UnexpectedDataType(expectedStates.ToArray(), actualState);

    }

    protected static void AssertReaderState(CborReader reader, CborReaderState expectedState)
    {
        var actualState = reader.PeekState();

        if (expectedState != actualState)
        {
            throw UnexpectedDataType(expectedState, actualState);
        }
    }

    protected static NotSupportedException CannotSerializeNullValue()
    {
        return new("Cannot serialize null object.");
    }

}


public abstract class CborConverter<T> : CborConverter
{
    private static readonly Lazy<bool> _handleNullLazy = new(static () => typeof(T).IsValueType);

    /// <summary>
    /// Indicates whether this converter can handle null values.
    /// </summary>
    /// <remarks>Default is <see cref="true"/> for value types and <see cref="false"/> for reference types.</remarks>
    public virtual bool HandleNull => _handleNullLazy.Value;

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);

    public abstract T? Read(CborReader reader, Type typeToConvert, CborSerializerOptions options);

    public abstract void Write(CborWriter writer, T? value, CborSerializerOptions options);

}

