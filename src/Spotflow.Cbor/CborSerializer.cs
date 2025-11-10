using System.Formats.Cbor;

using Spotflow.Cbor.Converters;

namespace Spotflow.Cbor;

public static class CborSerializer
{
    /// <summary>
    /// Checks whether the given CBOR data starts with the self-describe tag (0xd9d9f7).    
    /// </summary>
    /// <remarks>
    /// https://www.rfc-editor.org/rfc/rfc7049.html#section-2.4.5
    /// </remarks>
    public static bool StartsWithSelfDescribeTag(ReadOnlyMemory<byte> cbor, CborSerializerOptions? options = null)
    {
        if (cbor.IsEmpty)
        {
            return false;
        }

        options = ResolveOptions(options);

        var reader = options.LeaseReader(cbor);

        try
        {
            return StartsWithSelfDescribeTag(reader);
        }
        finally
        {
            options.ReturnReader(reader);
        }

    }

    /// <summary>
    /// Checks whether the given CBOR data starts with the self-describe tag (0xd9d9f7).    
    /// </summary>
    /// <remarks>
    /// https://www.rfc-editor.org/rfc/rfc7049.html#section-2.4.5
    /// </remarks>
    public static bool StartsWithSelfDescribeTag(CborReader reader)
    {
        if (reader.PeekState() is CborReaderState.Tag && reader.PeekTag() is CborTag.SelfDescribeCbor)
        {
            return true;
        }

        return false;
    }

    public static T? Deserialize<T>(ReadOnlyMemory<byte> cbor, CborSerializerOptions? options = null)
    {
        options = ResolveOptions(options);

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

        options = ResolveOptions(options);

        try
        {
            // Skip self-describe tag if present.

            if (StartsWithSelfDescribeTag(reader))
            {
                reader.ReadTag();
            }

            var converter = CborTypeInfo.ResolveReadConverterForType<T>(reader, options);

            var isNullableOfTOrReferenceType = CborTypeInfo.IsNullableOfTOrReferenceType<T>();

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
        options = ResolveOptions(options);

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

        options = ResolveOptions(options);

        var converter = CborTypeInfo.ResolveWriteConverterForType<T>(options);

        try
        {
            if (options.WriteSelfDescribeTag)
            {
                writer.WriteTag(CborTag.SelfDescribeCbor);
            }

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
        options = ResolveOptions(options);

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

    private static CborSerializerOptions ResolveOptions(CborSerializerOptions? options)
    {
        options ??= CborSerializerOptions.Default;
        options.MakeReadOnly();
        return options;
    }

    internal static T? ResolveValue<T>(bool isNullable, CborConverter<T> converter, CborReader reader, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        T? value;

        CborTag? tag = null;

        if (state is CborReaderState.Tag)
        {
            tag = reader.ReadTag();
        }

        if (state is CborReaderState.Null || (state is CborReaderState.SimpleValue && options.HandleUndefinedValuesAsNulls))
        {
            if (converter.HandleNull)
            {
                value = converter.Read(reader, typeof(T), tag: tag, options);
            }
            else if (isNullable)
            {
                if (state is CborReaderState.Null)
                {
                    reader.ReadNull();
                }
                else if (state is CborReaderState.SimpleValue)
                {
                    var readSimpleValue = reader.ReadSimpleValue();

                    if (readSimpleValue is not CborSimpleValue.Undefined)
                    {
                        throw new CborSerializerException($"Simple value '{readSimpleValue}' is not supported during deserialization.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid CBOR reader state.");
                }

                value = default; // Property is nullable, so default == null.
            }
            else
            {
                throw new CborSerializerException($"Null CBOR value cannot be converted to '{typeof(T)}'.");
            }
        }
        else
        {
            value = converter.Read(reader, typeof(T), tag: tag, options);
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
