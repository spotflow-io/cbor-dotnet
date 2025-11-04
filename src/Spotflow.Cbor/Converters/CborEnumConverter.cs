using System.Formats.Cbor;
using System.Linq.Expressions;

namespace Spotflow.Cbor.Converters;

internal class CborEnumConverter<T, TUnderlying> : CborConverter<T> where T : struct, Enum where TUnderlying : struct
{
    private readonly TypeCode _typeCode;
    private readonly bool _serializeAsString;
    private readonly bool _allowDeserializationFromString;
    private readonly Func<ulong, TUnderlying> _fromUnsignedValueToUnderlyingType;
    private readonly Func<long, TUnderlying> _fromSignedValueToUnderlyingType;
    private readonly bool _isUnderlyingTypeUnsigned;
    private readonly Func<TUnderlying, T> _castFromUnderlying;
    private readonly bool _caseSensitive;

    public CborEnumConverter(bool serializeAsString, bool allowDeserializationFromString, bool caseSensitive)
    {
        var type = typeof(T);
        var underlyingType = Enum.GetUnderlyingType(type);

        if (underlyingType != typeof(TUnderlying))
        {
            throw new InvalidOperationException($"The specified underlying type '{typeof(TUnderlying)}' does not match the actual underlying type '{underlyingType}' of enum '{type}'.");
        }

        _typeCode = Type.GetTypeCode(underlyingType);
        _isUnderlyingTypeUnsigned = _typeCode is TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64;
        _serializeAsString = serializeAsString;
        _allowDeserializationFromString = allowDeserializationFromString;
        _fromUnsignedValueToUnderlyingType = CompileFromUnsignedValueToUnderlyingType(_typeCode);
        _fromSignedValueToUnderlyingType = CompileFromSignedValueToUnderlyingType(_typeCode);
        _castFromUnderlying = CompileCastFromUnderlyingType();
        _caseSensitive = caseSensitive;
    }

    public override bool HandleNull => false;

    public override T Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.UnsignedInteger)
        {
            var rawValue = reader.ReadUInt64();

            var underlyingValue = _fromUnsignedValueToUnderlyingType(rawValue);

            var value = _castFromUnderlying(underlyingValue);

            return value;
        }

        if (state is CborReaderState.NegativeInteger)
        {
            if (_isUnderlyingTypeUnsigned)
            {
                throw new CborDataSerializationException($"Cannot convert negative integer to unsigned enum underlying type: {_typeCode}.");
            }

            var rawValue = reader.ReadInt64();

            var underlyingValue = _fromSignedValueToUnderlyingType(rawValue);

            var value = _castFromUnderlying(underlyingValue);

            return value;
        }

        if (_allowDeserializationFromString)
        {
            if (state is CborReaderState.TextString)
            {
                var value = reader.ReadTextString();

                if (!Enum.TryParse(typeToConvert, value, ignoreCase: (_caseSensitive is not true), out var enumValue))
                {
                    throw new CborDataSerializationException($"Invalid text value for enum '{typeToConvert.Name}': '{value}'.");
                }

                return (T) enumValue;
            }

            throw UnexpectedDataType([CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger, CborReaderState.TextString], state);

        }
        else
        {
            throw UnexpectedDataType([CborReaderState.UnsignedInteger, CborReaderState.NegativeInteger], state);
        }
    }

    public override void Write(CborWriter writer, T value, CborSerializerOptions options)
    {
        if (_serializeAsString)
        {
            var textValue = Enum.GetName(value);

            if (textValue is null)
            {
                throw new CborSerializerException($"Unable to convert enum value '{value}' to its string representation.");
            }

            writer.WriteTextString(textValue);
        }
        else
        {
            if (_isUnderlyingTypeUnsigned)
            {
                writer.WriteUInt64(Convert.ToUInt64(value));
            }
            else
            {
                writer.WriteInt64(Convert.ToInt64(value));
            }
        }
    }

    private static Func<TUnderlying, T> CompileCastFromUnderlyingType()
    {
        var operand = Expression.Parameter(typeof(TUnderlying), "value");
        var cast = Expression.ConvertChecked(operand, typeof(T));
        var lambda = Expression.Lambda<Func<TUnderlying, T>>(cast, operand);
        return lambda.Compile();
    }

    private static Func<ulong, TUnderlying> CompileFromUnsignedValueToUnderlyingType(TypeCode typeCode)
    {
        object fromUnsigned = typeCode switch
        {
            TypeCode.SByte => (ulong u) => Convert.ToSByte(u),
            TypeCode.Byte => (ulong u) => Convert.ToByte(u),
            TypeCode.Int16 => (ulong u) => Convert.ToInt16(u),
            TypeCode.UInt16 => (ulong u) => Convert.ToUInt16(u),
            TypeCode.Int32 => (ulong u) => Convert.ToInt32(u),
            TypeCode.UInt32 => (ulong u) => Convert.ToUInt32(u),
            TypeCode.Int64 => (ulong u) => Convert.ToInt64(u),
            TypeCode.UInt64 => (ulong u) => Convert.ToUInt64(u),

            _ => throw new InvalidOperationException($"Unsupported enum underlying type: {typeCode}.")
        };

        return (Func<ulong, TUnderlying>) fromUnsigned;
    }

    private static Func<long, TUnderlying> CompileFromSignedValueToUnderlyingType(TypeCode typeCode)
    {
        object fromSigned = typeCode switch
        {
            TypeCode.SByte => (long s) => Convert.ToSByte(s),
            TypeCode.Byte => (long s) => throwNotSupported<byte>(),
            TypeCode.Int16 => (long s) => Convert.ToInt16(s),
            TypeCode.UInt16 => (long s) => throwNotSupported<ushort>(),
            TypeCode.Int32 => (long s) => Convert.ToInt32(s),
            TypeCode.UInt32 => (long s) => throwNotSupported<uint>(),
            TypeCode.Int64 => (long s) => Convert.ToInt64(s),
            TypeCode.UInt64 => (long s) => throwNotSupported<ulong>(),

            _ => throw new InvalidOperationException($"Unsupported enum underlying type: {typeCode}.")
        };

        return (Func<long, TUnderlying>) fromSigned;

        static TX throwNotSupported<TX>() where TX : struct
        {
            throw new InvalidOperationException("Conversion from negative integer to unsigned enum underlying type is not expected.");
        }
    }


}
