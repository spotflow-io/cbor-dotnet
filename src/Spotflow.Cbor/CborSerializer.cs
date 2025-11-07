using System.Formats.Cbor;

using Spotflow.Cbor.Converters;

namespace Spotflow.Cbor;

public static class CborSerializer
{
    public static T? Deserialize<T>(ReadOnlyMemory<byte> cbor, CborSerializerOptions? options = null)
    {
        options ??= CborSerializerOptions.Default;

        options.MakeReadOnly();

        var reader = options.LeaseReader(cbor);

        try
        {
            return Deserialize<T>(reader, options);
        }
        finally
        {
            options.ReturnReader(reader);
        }
    }

    public static T? Deserialize<T>(CborReader reader, CborSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var initBytesRemaining = reader.BytesRemaining;

        options ??= CborSerializerOptions.Default;

        options.MakeReadOnly();

        var converter = CborTypeInfo.ResolveReadConverterForType<T>(reader, options);

        var isNullableOfTOrReferenceType = CborTypeInfo.IsNullableOfTOrReferenceType<T>();

        try
        {
            return ResolveValue(isNullable: isNullableOfTOrReferenceType, converter, reader, options);
        }
        catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
        {
            var currentByte = initBytesRemaining - reader.BytesRemaining;

            throw CborSerializerException.WrapWithPositionInfo(ex, currentByte, reader.CurrentDepth);
        }
    }

    public static byte[] Serialize<T>(T value, CborSerializerOptions? options = null)
    {
        options ??= CborSerializerOptions.Default;

        options.MakeReadOnly();

        var writer = options.LeaseWriter();
        try
        {
            return Serialize(value, writer, options);
        }
        finally
        {
            options.ReturnWriter(writer);
        }

    }

    public static byte[] Serialize<T>(T value, CborWriter writer, CborSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);

        options ??= CborSerializerOptions.Default;

        options.MakeReadOnly();

        var converter = CborTypeInfo.ResolveWriteConverterForType<T>(options);

        try
        {
            converter.Write(writer, value, options);
        }
        catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
        {
            throw CborSerializerException.WrapWithPositionInfo(ex, writer.BytesWritten, writer.CurrentDepth);
        }

        return writer.Encode();
    }

    public static bool TrySerialize<T>(T value, Span<byte> buffer, out int bytesWritten, CborSerializerOptions? options = null)
    {
        options ??= CborSerializerOptions.Default;

        options.MakeReadOnly();

        var writer = options.LeaseWriter();
        try
        {
            var converter = CborTypeInfo.ResolveWriteConverterForType<T>(options);
            converter.Write(writer, value, options);
            return writer.TryEncode(buffer, out bytesWritten);
        }
        finally
        {
            options.ReturnWriter(writer);
        }
    }

    internal static T? ResolveValue<T>(bool isNullable, CborConverter<T> converter, CborReader reader, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        T? value;

        if (state is CborReaderState.Null)
        {
            if (converter.HandleNull)
            {
                value = converter.Read(reader, typeof(T), options);
            }
            else if (isNullable)
            {
                reader.ReadNull();
                value = default; // Property is nullable, so default == null.
            }
            else
            {
                throw new CborSerializerException($"Null CBOR value cannot be converted to '{typeof(T)}'.");
            }
        }
        else
        {
            value = converter.Read(reader, typeof(T), options);
        }

        if (value is not null)
        {
            return value;
        }

        if (isNullable)
        {
            return value;
        }

        throw new CborSerializerException($"Null CBOR value cannot be assigned to '{typeof(T)}'.");

    }
}
