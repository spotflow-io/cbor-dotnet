using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Spotflow.Cbor.Converters;

internal static class CborObjectConverterFactory
{
    private static readonly ConcurrentDictionary<(Type, CborSerializerOptions), CborConverter> _converterCache = new();

    private static readonly MethodInfo _createObjectFactoryMethod;

    private static readonly ConcurrentDictionary<Type, MethodInfo> _generateCreateObjectFactoryMethodCache = new();

    static CborObjectConverterFactory()
    {
        var createObjectFactoryMethod = typeof(CborObjectConverterFactory)
            .GetMethod(nameof(CreateObjectFactory), BindingFlags.NonPublic | BindingFlags.Static);

        if (createObjectFactoryMethod is null)
        {
            throw new InvalidOperationException($"Failed to retrieve '{nameof(CreateObjectFactory)}' method info.");
        }

        _createObjectFactoryMethod = createObjectFactoryMethod;
    }

    public static CborConverter CreateConverter(Type typeToConvert, CborSerializerOptions options)
    {
        return _converterCache.GetOrAdd((typeToConvert, options), CreateInstance);
    }

    private static CborConverter CreateInstance((Type, CborSerializerOptions) args)
    {
        var (type, _) = args;

        var genericCreateObjectFactoryMethod = _generateCreateObjectFactoryMethodCache.GetOrAdd(type, static t => _createObjectFactoryMethod.MakeGenericMethod(t));

        var objectFactory = genericCreateObjectFactoryMethod.Invoke(null, [type]);

        var converterType = typeof(CborObjectConverter<>).MakeGenericType(type);

        var converter = (CborConverter?) Activator.CreateInstance(converterType, [objectFactory]);

        if (converter is null)
        {
            throw new InvalidOperationException($"Failed to create instance of converter '{converterType}'.");
        }

        return converter;
    }

    private static Func<T> CreateObjectFactory<T>(Type type)
    {
        var constructorExpression = Expression.New(type);

        var constructorExpressionLambda = Expression.Lambda<Func<T>>(constructorExpression);

        return constructorExpressionLambda.Compile();
    }
}




