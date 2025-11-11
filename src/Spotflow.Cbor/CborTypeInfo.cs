using System.Collections.Concurrent;
using System.Formats.Cbor;
using System.Reflection;

using Spotflow.Cbor.Converters;

namespace Spotflow.Cbor;

internal static class CborTypeInfo
{
    private static readonly ConcurrentDictionary<(Type, CborSerializerOptions), CborConverter?> _cachedConvertersForTypes = new();
    private static readonly ConcurrentDictionary<(PropertyInfo, CborSerializerOptions), CborConverter?> _cachedConvertersForProperties = new();
    private static readonly ConcurrentDictionary<(Type, CborSerializerOptions), CborConverter> _fallbackConverterCache = new();

    private static readonly CborConverter[] _builtInConvertersForPrimitiveTypes = PrepareBuiltInConvertersForPrimitiveTypes();

    private static readonly Type _genericConverterOfTType = typeof(CborConverter<>);

    private static readonly ConcurrentDictionary<Type, bool> _isNullableOfTOrReferenceTypeCache = new();

    public static bool IsNullableOfTOrReferenceType<T>()
    {
        var type = typeof(T);

        return _isNullableOfTOrReferenceTypeCache.GetOrAdd(type, static (type) =>
        {
            if (!type.IsValueType)
            {
                return true;
            }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        });
    }

    public static CborConverter<T> ResolveReadConverterForType<T>(CborReader reader, CborSerializerOptions options)
    {
        var converter = ResolveConverterForType(typeof(T), reader, options);
        return GetTypedConverter<T>(converter);
    }

    public static CborConverter<T> ResolveWriteConverterForType<T>(CborSerializerOptions options)
    {
        var converter = ResolveConverterForType(typeof(T), reader: null, options);
        return GetTypedConverter<T>(converter);
    }

    private static CborConverter ResolveConverterForType(Type type, CborReader? reader, CborSerializerOptions options)
    {
        options.MakeReadOnly();
        var converter = _cachedConvertersForTypes.GetOrAdd((type, options), ResolveConverterForTypeCore);
        return converter ?? ResolveFallbackConverter(type, reader, options);
    }

    private static CborConverter? ResolveConverterForTypeCore((Type, CborSerializerOptions) args)
    {
        var (type, options) = args;

        CborConverter? converter = null;

        converter ??= FindConverterForType(type, options.Converters);

        if (converter is null)
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(type) ?? type;
            var typeAttributes = underlyingNullableType.GetCustomAttributes();
            converter = FindConverterFromAttributes(typeAttributes, type);
        }

        converter ??= FindConverterForType(type, _builtInConvertersForPrimitiveTypes);

        if (converter is null)
        {
            return null;
        }

        converter = ResolveConverterFactory(type, converter, options);

        converter = ResolveNullableValueTypeConverter(type, converter);

        AssertConverterType(converter, type);

        return converter;
    }

    public static CborConverter<T> ResolveReadConverterForProperty<T>(PropertyInfo propertyInfo, CborReader reader, CborSerializerOptions options)
    {
        var converter = ResolveConverterForProperty(propertyInfo, reader, options);
        return GetTypedConverter<T>(converter);
    }

    public static CborConverter<T> ResolveWriteConverterForProperty<T>(PropertyInfo propertyInfo, CborSerializerOptions options)
    {
        var converter = ResolveConverterForProperty(propertyInfo, null, options);
        return GetTypedConverter<T>(converter);
    }

    private static CborConverter ResolveConverterForProperty(PropertyInfo propertyInfo, CborReader? reader, CborSerializerOptions options)
    {
        options.MakeReadOnly();
        var converter = _cachedConvertersForProperties.GetOrAdd((propertyInfo, options), ResolveConverterForPropertyCore);
        return converter ?? ResolveFallbackConverter(propertyInfo.PropertyType, reader, options);
    }

    private static CborConverter? ResolveConverterForPropertyCore((PropertyInfo, CborSerializerOptions) args)
    {
        var (propertyInfo, options) = args;

        var type = propertyInfo.PropertyType;

        var converter = FindConverterFromAttributes(propertyInfo.GetCustomAttributes(), valueType: type);

        if (converter is not null)
        {
            converter = ResolveConverterFactory(type, converter, options);

            converter = ResolveNullableValueTypeConverter(type, converter);

            AssertConverterType(converter, type);

            return converter;
        }

        return ResolveConverterForTypeCore((propertyInfo.PropertyType, options));
    }

    private static CborConverter<T> GetTypedConverter<T>(CborConverter converter)
    {
        if (converter is not CborConverter<T> typedConverter)
        {
            throw new InvalidOperationException($"Converter '{converter.GetType()}' is not of expected type '{typeof(CborConverter<T>)}'.");
        }

        return typedConverter;
    }

    private static CborConverter ResolveFallbackConverter(Type type, CborReader? reader, CborSerializerOptions options)
    {
        if (type == typeof(Type))
        {
            throw new NotSupportedException($"Serialization or deserialization of '{typeof(Type).FullName}' is not supported.");
        }

        if (reader is not null)
        {
            var state = reader.PeekState();

            if (state is not CborReaderState.StartMap and not CborReaderState.Null)
            {
                throw new CborSerializerException($"CBOR value '{state}' could not be converted to '{type}'.");
            }
        }

        options.MakeReadOnly();

        return _fallbackConverterCache.GetOrAdd((type, options), static (args) =>
        {
            var (type, options) = args;
            return CborObjectConverterFactory.CreateConverter(type, options);
        });
    }

    private static CborConverter ResolveNullableValueTypeConverter(Type type, CborConverter converter)
    {
        var underlyingTypeOfNullable = Nullable.GetUnderlyingType(type);

        if (underlyingTypeOfNullable is null)
        {
            return converter;
        }

        var converterBaseType = converter.GetType().BaseType;

        while (converterBaseType != null && converterBaseType != typeof(object))
        {
            if (converterBaseType.IsGenericType && converterBaseType.GetGenericTypeDefinition() == typeof(CborConverter<>))
            {
                break;
            }

            converterBaseType = converterBaseType?.BaseType;
        }

        if (converterBaseType is null)
        {
            throw new InvalidOperationException("Converter base type not found.");
        }

        var converterValueType = converterBaseType.GetGenericArguments()[0];

        var converterUnderlyingTypeOfNullable = Nullable.GetUnderlyingType(converterValueType);

        if (converterUnderlyingTypeOfNullable is not null)
        {
            // The converter already handles nullable types

            return converter;
        }

        var adapterConvertorType = typeof(NullableValueTypeAdapterConverter<>).MakeGenericType(underlyingTypeOfNullable);

        if (Activator.CreateInstance(adapterConvertorType, converter) is not CborConverter adapterConverter)
        {
            throw new NotSupportedException($"Failed to create instance of converter type '{type.FullName}'.");
        }

        return adapterConverter;
    }


    private static CborConverter ResolveConverterFactory(Type type, CborConverter converter, CborSerializerOptions options)
    {
        if (converter is CborConverterFactory factory)
        {
            converter = factory.CreateConverter(type, options);

            if (converter is CborConverterFactory)
            {
                throw new NotSupportedException($"Converter factory '{converter.GetType().FullName}' returned another converter factory for type '{type.FullName}'.");
            }
        }

        return converter;
    }

    private static CborConverter? FindConverterForType(Type type, IEnumerable<CborConverter> converters)
    {
        foreach (var converter in converters)
        {
            if (CanConvertTypeOrUnderlyingTypeOfNullable(converter, type))
            {
                return converter;
            }
        }

        return null;
    }

    private static bool CanConvertTypeOrUnderlyingTypeOfNullable(CborConverter converter, Type type)
    {
        if (converter.CanConvert(type))
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        var underlyingTypeOfNullable = Nullable.GetUnderlyingType(type);

        if (underlyingTypeOfNullable is null)
        {
            return false;
        }

        return converter.CanConvert(underlyingTypeOfNullable);
    }

    private static CborConverter? FindConverterFromAttributes(IEnumerable<Attribute> attributes, Type valueType)
    {
        var attribute = attributes.Where(filterConverterAttribute).SingleOrDefault();

        if (attribute is null)
        {
            return null;
        }

        var converterType = attribute.GetType().GenericTypeArguments.Single();

        if (Activator.CreateInstance(converterType) is not CborConverter converter)
        {
            throw new NotSupportedException($"Failed to create instance of converter type '{converterType.FullName}'.");
        }

        if (!CanConvertTypeOrUnderlyingTypeOfNullable(converter, valueType))
        {
            throw new NotSupportedException($"Converter '{converterType.FullName}' cannot resolve value of type '{valueType.FullName}'.");
        }

        return converter;

        static bool filterConverterAttribute(Attribute a)
        {
            return a.GetType().IsGenericType && a.GetType().GetGenericTypeDefinition() == typeof(CborConverterAttribute<>);
        }
    }

    private static void AssertConverterType(CborConverter converter, Type valueType)
    {
        var converterType = converter.GetType();

        if (!CanConvertTypeOrUnderlyingTypeOfNullable(converter, valueType))
        {
            throw new NotSupportedException($"Converter '{converterType.FullName}' cannot convert value of type '{valueType.FullName}'.");
        }

        var baseType = converterType;

        while (baseType is not null && baseType != typeof(object))
        {
            if (baseType.IsGenericType)
            {
                var baseTypeGeneric = baseType.GetGenericTypeDefinition();

                if (baseTypeGeneric == _genericConverterOfTType)
                {
                    var converterValueType = baseType.GetGenericArguments()[0];

                    if (!valueType.IsAssignableFrom(converterValueType))
                    {
                        throw new NotSupportedException($"Converter type '{converterType.FullName}' returing '{converterValueType.FullName}' is not compatible with value type '{valueType.FullName}'.");
                    }

                    return;
                }
            }

            baseType = baseType.BaseType;
        }

        throw new NotSupportedException($"Converter type '{converterType.FullName}' does not inherit from '{_genericConverterOfTType.FullName}'.");
    }

    private static CborConverter[] PrepareBuiltInConvertersForPrimitiveTypes()
    {
        return [
            new CborStringConverter(),
            new CborIntegerConverter.Byte(),
            new CborIntegerConverter.SByte(),
            new CborIntegerConverter.Int16(),
            new CborIntegerConverter.UInt16(),
            new CborIntegerConverter.Int32(),
            new CborIntegerConverter.UInt32(),
            new CborIntegerConverter.Int64(),
            new CborIntegerConverter.UInt64(),
            new CborIntegerConverter.Int128(),
            new CborIntegerConverter.UInt128(),
            new CborBigIntegerConverter(),
            new CborBooleanConverter(),
            new CborFloatingPointConverter.Half(),
            new CborFloatingPointConverter.Single(),
            new CborFloatingPointConverter.Double(),
            new CborNumberEnumConverter(),
            new CborDateTimeConverter(),
            new CborDateTimeOffsetConverter(),
            new CborTimeOnlyConverter(),
            new CborDateOnlyConverter(),
            new CborGuidConverter(),
            new CborUriConverter(),
            new CborByteStringConverter.Array(),
            new CborByteStringConverter.Memory(),
            new CborByteStringConverter.ReadOnlyMemory(),
            new CborDictionaryConverter(),
            new CborConcurrentDictionaryConverter(),
            new CborFrozenDictionaryConverter(),
            new CborArrayConverter(),
            new CborListConverter(),
        ];
    }


}


