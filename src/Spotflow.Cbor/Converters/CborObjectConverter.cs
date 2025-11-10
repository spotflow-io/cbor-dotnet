using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Cbor;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Spotflow.Cbor.Converters;

internal class CborObjectConverter<TObject>(Func<TObject> factory) : CborConverter<TObject>
{
    private static readonly ConcurrentDictionary<CborSerializerOptions, ObjectDescriptor> _descriptors = new();

    public override bool HandleNull => false;

    public override TObject? Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var initialDepth = reader.CurrentDepth;

        options.AssertMaxDepth(initialDepth);

        var objectDescriptor = ResolveDescriptor(options);

        var initState = reader.PeekState();

        if (initState is not CborReaderState.StartMap)
        {
            throw new CborSerializerException($"Expected start of CBOR map representing object but found '{initState}'.");
        }

        _ = reader.ReadStartMap();

        var obj = factory();

        HashSet<PropertyDescriptor>? processedProperties = null;

        while (reader.PeekState() != CborReaderState.EndMap && reader.PeekState() != CborReaderState.Finished)
        {
            var property = ReadPropertyKey(reader, out var textName, out var numericName, objectDescriptor);

            if (property is null)
            {
                if (options.UnmappedMemberHandling is CborUnmappedMemberHandling.Disallow)
                {
                    var propertyIdentifier = FormatPropertyDisplayName(textName, numericName);
                    throw new CborSerializerException($"Unmapped property '{propertyIdentifier}'.");
                }

                reader.SkipValue();
                continue;
            }

            try
            {
                if (!property.CanSet)
                {
                    reader.SkipValue();
                    continue;
                }

                processedProperties ??= [];
                processedProperties.Add(property);

                property.Read(ref obj, reader, options);

            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentProperty(ex, initialDepth, objectDescriptor, property);
            }

        }

        if (objectDescriptor.HasRequiredProperties)
        {
            var requiredProperties = objectDescriptor.Properties.Where(p => p.IsRequired).ToList();
            List<PropertyDescriptor>? missingProperties = null;

            foreach (var requiredProperty in requiredProperties)
            {
                if (processedProperties is null || !processedProperties.Contains(requiredProperty))
                {
                    missingProperties ??= [];
                    missingProperties.Add(requiredProperty);
                }
            }

            if (missingProperties?.Count > 0)
            {
                var missingPropertiesQuoted = missingProperties.Select(p => $"'{FormatPropertyDisplayName(p.TextName, p.NumericName)}'");
                var missingPropertiesFormatted = string.Join(", ", missingPropertiesQuoted);

                throw new CborSerializerException($"Required properties are missing: {missingPropertiesFormatted}.");
            }

        }

        var finalState = reader.PeekState();

        if (finalState is not CborReaderState.EndMap)
        {
            throw new CborSerializerException($"Expected end of CBOR map for object but found '{finalState}'.");
        }

        reader.ReadEndMap();

        return obj;

    }

    private static Exception EnrichWithParentProperty(Exception ex, int objectDepth, ObjectDescriptor objectDescriptor, PropertyDescriptor propertyDescriptor)
    {
        var propertyDisplayName = FormatPropertyDisplayName(propertyDescriptor.TextName, propertyDescriptor.NumericName);
        var name = $"#{objectDepth}: {propertyDisplayName} ({objectDescriptor.ObjectType.FullName})";

        return CborSerializerException.WrapWithParentScope(ex, name);
    }

    private static string FormatPropertyDisplayName(string? textName, int? numericName)
    {
        if (textName is not null)
        {
            if (numericName is not null)
            {
                return $"{textName} {{{numericName.Value.ToString(NumberFormatInfo.InvariantInfo)}}}";
            }

            return textName;
        }

        if (numericName is not null)
        {
            return numericName.Value.ToString(NumberFormatInfo.InvariantInfo);
        }

        throw new InvalidOperationException("Both text and numeric property names are null.");
    }

    private static ObjectDescriptor ResolveDescriptor(CborSerializerOptions options)
    {
        options.MakeReadOnly();

        return _descriptors.GetOrAdd(options, static options => ObjectDescriptor.Create(typeof(TObject), options));

    }

    private static PropertyDescriptor? ReadPropertyKey(CborReader reader, out string? textName, out int? numericName, ObjectDescriptor descriptor)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.TextString)
        {
            var propertyName = reader.ReadTextString();

            textName = propertyName;
            numericName = null;

            return descriptor.FindPropertyByTextName(propertyName);
        }

        if (state is CborReaderState.UnsignedInteger)
        {
            var propertyNumericName = reader.ReadInt32();

            textName = null;
            numericName = propertyNumericName;

            return descriptor.FindPropertyByNumericName(propertyNumericName);
        }

        throw new CborSerializerException(
            $"Unsupported CBOR type of object property name. Expected '{CborReaderState.TextString}' or '{CborReaderState.UnsignedInteger}', got '{state}'.");

    }

    public override void Write(CborWriter writer, TObject? value, CborSerializerOptions options)
    {
        options.AssertMaxDepth(writer.CurrentDepth);

        if (value is null)
        {
            throw CannotSerializeNullValue();
        }

        var descriptor = ResolveDescriptor(options);

        int? count = options.DefaultIgnoreCondition is CborIgnoreCondition.Never ? descriptor.Properties.Count : null;

        writer.WriteStartMap(count);

        foreach (var property in descriptor.Properties)
        {
            try
            {
                property.Write(ref value, writer, options);
            }
            catch (Exception ex) when (CborSerializerException.IsRecognizedException(ex))
            {
                throw EnrichWithParentProperty(ex, writer.CurrentDepth, descriptor, property);
            }
        }

        writer.WriteEndMap();
    }

    private class ObjectDescriptor(
        Type type,
        IReadOnlyList<PropertyDescriptor> properties,
        FrozenDictionary<string, PropertyDescriptor> propertiesByName,
        FrozenDictionary<int, PropertyDescriptor> propertiesByNumericName
        )
    {

        public Type ObjectType => type;

        public bool HasRequiredProperties { get; } = properties.Any(p => p.IsRequired);

        public IReadOnlyList<PropertyDescriptor> Properties => properties;

        public static ObjectDescriptor Create(Type type, CborSerializerOptions options)
        {
            if (type == typeof(object))
            {
                throw new NotSupportedException("Cannot serialize or deserialize objects of type 'object'.");
            }

            var propertyTextNameComparer = options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

            var properties = new List<PropertyDescriptor>();
            var propertiesByTextName = new Dictionary<string, PropertyDescriptor>(propertyTextNameComparer);
            var propertiesByNumericName = new Dictionary<int, PropertyDescriptor>();

            var nullabilityInfoContext = new NullabilityInfoContext();

            foreach (var property in type.GetProperties())
            {
                var propertyDescriptor = PropertyDescriptor.Create(property, nullabilityInfoContext, options);

                var (textName, numericName) = (propertyDescriptor.TextName, propertyDescriptor.NumericName);

                if (!propertiesByTextName.TryAdd(textName, propertyDescriptor))
                {
                    throw new NotSupportedException($"Duplicate property name '{FormatPropertyDisplayName(textName, numericName)}'.");
                }

                if (numericName.HasValue && !propertiesByNumericName.TryAdd(numericName.Value, propertyDescriptor))
                {
                    throw new NotSupportedException($"Duplicate property name '{FormatPropertyDisplayName(textName, numericName)}'.");
                }

                properties.Add(propertyDescriptor);
            }

            return new(
                type,
                properties,
                propertiesByTextName.ToFrozenDictionary(propertyTextNameComparer),
                propertiesByNumericName.ToFrozenDictionary()
                );


        }

        public PropertyDescriptor? FindPropertyByTextName(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return propertiesByName.GetValueOrDefault(name);
        }

        public PropertyDescriptor? FindPropertyByNumericName(int name)
        {
            return propertiesByNumericName.GetValueOrDefault(name);
        }
    }

    private abstract class PropertyDescriptor(string textName, int? numericName)
    {
        public string TextName => textName;
        public int? NumericName => numericName;
        public abstract bool IsRequired { get; }

        public abstract bool CanSet { get; }
        public abstract void Read(ref TObject obj, CborReader reader, CborSerializerOptions options);

        public abstract void Write(ref TObject obj, CborWriter writer, CborSerializerOptions options);

        public static PropertyDescriptor Create(PropertyInfo property, NullabilityInfoContext nullabilityInfoContext, CborSerializerOptions options)
        {
            var (numericName, textName) = ParsePropertyAttribute(property);

            textName ??= options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

            var nullabilityInfo = nullabilityInfoContext.Create(property);
            var hasNullableAnnotation = nullabilityInfo.ReadState is NullabilityState.Nullable;
            var propertyDescriptorType = typeof(PropertyDescriptor<>).MakeGenericType(typeof(TObject), property.PropertyType);

            var propertyDescriptorArgs = new object?[] { property, textName, numericName, hasNullableAnnotation };
            var propertyDescriptor = (PropertyDescriptor?) Activator.CreateInstance(propertyDescriptorType, propertyDescriptorArgs);

            if (propertyDescriptor is null)
            {
                throw new InvalidOperationException("Property descriptor not created.");
            }

            return propertyDescriptor;
        }

        private static (int? NumericName, string? TextName) ParsePropertyAttribute(PropertyInfo clrProperty)
        {
            var attribute = clrProperty.GetCustomAttribute<CborPropertyAttribute>();

            if (attribute is null)
            {
                return (null, null);
            }

            int? numericName = attribute.NumericName >= 0 ? attribute.NumericName : null;
            var textName = !string.IsNullOrWhiteSpace(attribute.TextName) ? attribute.TextName : null;

            return (numericName, textName);
        }

    }

    private class PropertyDescriptor<TProperty>(
        PropertyInfo property,
        string textName,
        int? numericName,
        bool hasNullableAnnotation
        ) : PropertyDescriptor(textName, numericName)
    {

        private readonly SetValueDelegate? _setValueDelegate = GenerateSetValueDelegate(property);
        private readonly GetValueDelegate _getValueDelegate = GenerateGetValueDelegate(property);
        private readonly bool _isReferenceType = !property.PropertyType.IsValueType;

        public override bool IsRequired { get; } = property.GetCustomAttribute<RequiredMemberAttribute>() is not null;
        [MemberNotNullWhen(true, nameof(_setValueDelegate))]
        public override bool CanSet => _setValueDelegate is not null;
        public override void Read(ref TObject obj, CborReader reader, CborSerializerOptions options)
        {
            if (!CanSet)
            {
                throw new InvalidOperationException("Property cannot be set.");
            }

            var converter = CborTypeInfo.ResolveReadConverterForProperty<TProperty>(property, reader, options);

            var isNullable = (!_isReferenceType && hasNullableAnnotation) || (_isReferenceType && (hasNullableAnnotation || !options.RespectNullableAnnotations));

            var value = CborSerializer.ResolveValue(isNullable, converter, reader, options);

            _setValueDelegate(ref obj, value);
        }

        public override void Write(ref TObject obj, CborWriter writer, CborSerializerOptions options)
        {
            var converter = CborTypeInfo.ResolveWriteConverterForProperty<TProperty>(property, options);

            var value = _getValueDelegate(ref obj);

            if (value is null && options.DefaultIgnoreCondition is CborIgnoreCondition.WhenWritingNull)
            {
                return;
            }

            if (NumericName is not null && options.PreferNumericPropertyNames)
            {
                writer.WriteInt32(NumericName.Value);
            }
            else
            {
                writer.WriteTextString(TextName);
            }

            if (value is null && !converter.HandleNull)
            {
                writer.WriteNull();
            }
            else
            {
                converter.Write(writer, value, options);
            }
        }

        private static SetValueDelegate? GenerateSetValueDelegate(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            var objParam = Expression.Parameter(typeof(TObject).MakeByRefType(), "obj");
            var valueParam = Expression.Parameter(typeof(TProperty), "value");
            var propertyExpression = Expression.Property(objParam, propertyInfo);
            var assignExpression = Expression.Assign(propertyExpression, valueParam);
            var lambda = Expression.Lambda<SetValueDelegate>(assignExpression, objParam, valueParam);
            return lambda.Compile();
        }

        private static GetValueDelegate GenerateGetValueDelegate(PropertyInfo propertyInfo)
        {
            var objParam = Expression.Parameter(typeof(TObject).MakeByRefType(), "obj");
            var propertyExpression = Expression.Property(objParam, propertyInfo);
            var lambda = Expression.Lambda<GetValueDelegate>(propertyExpression, objParam);
            return lambda.Compile();
        }

        private delegate void SetValueDelegate(ref TObject obj, TProperty? value);
        private delegate TProperty? GetValueDelegate(ref TObject obj);
    }



}


