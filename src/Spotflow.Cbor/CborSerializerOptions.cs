using System.Formats.Cbor;

using Spotflow.Cbor.Converters;

namespace Spotflow.Cbor;

public class CborSerializerOptions
{
    public static CborSerializerOptions Default { get; }

    private readonly Lock _lock = new();

    private bool _isReadOnly = false;

    private readonly ObjectPool<CborReader> _readersPool;
    private readonly ObjectPool<CborWriter> _writersPool;

    public static int DefaultMaxDepth { get; } = 64;

    static CborSerializerOptions()
    {
        Default = new CborSerializerOptions();
        Default.MakeReadOnly();
    }

    public CborSerializerOptions()
    {
        _readersPool = new(static o => new(ReadOnlyMemory<byte>.Empty, o.ConformanceMode), this);
        _writersPool = new(static o => new(o.ConformanceMode, convertIndefiniteLengthEncodings: o.ConvertIndefiniteLengthEncodings), this);
    }

    public CborIgnoreCondition DefaultIgnoreCondition
    {
        get;
        set
        {
            AssertNotReadOnly();

            if (value is CborIgnoreCondition.Always)
            {
                throw new ArgumentException($"'{CborIgnoreCondition.Always}' is not valid for '{nameof(DefaultIgnoreCondition)}'. Use the '{nameof(CborIgnoreAttribute)}' attribute to ignore specific properties.", nameof(value));
            }

            field = value;
        }
    } = CborIgnoreCondition.Never;

    public bool RespectNullableAnnotations
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    public CborConformanceMode ConformanceMode
    {
        get; set
        {

            AssertNotReadOnly();
            field = value;
        }
    } = CborConformanceMode.Strict;

    public bool ConvertIndefiniteLengthEncodings
    {
        get; set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    private readonly List<CborConverter> _converters = [];

    public IList<CborConverter> Converters => _converters;

    public bool PreferNumericPropertyNames
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = true;

    public CborNamingPolicy? PropertyNamingPolicy
    {
        get; set
        {
            AssertNotReadOnly();
            field = value;
        }
    }

    public CborUnmappedMemberHandling UnmappedMemberHandling
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = CborUnmappedMemberHandling.Skip;

    public CborBooleanHandling BooleanHandling
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = CborBooleanHandling.Strict;

    public CborNumberHandling NumberHandling
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = CborNumberHandling.Strict;

    public bool PropertyNameCaseInsensitive
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    public int MaxDepth
    {
        get;
        set
        {
            AssertNotReadOnly();

            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);

            field = value;
        }
    } = 0;

    public bool HandleUndefinedValuesAsNulls
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    public bool WriteSelfDescribeTag
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    public bool WriteDateTimeStringTag
    {
        get;
        set
        {
            AssertNotReadOnly();
            field = value;
        }
    } = false;

    public void MakeReadOnly()
    {
        if (_isReadOnly) // Optimistic check
        {
            return;
        }

        using var lockScope = _lock.EnterScope();

        _isReadOnly = true;
    }

    internal CborReader LeaseReader(ReadOnlyMemory<byte> cbor)
    {
        var reader = _readersPool.Allocate();
        reader.Reset(cbor);
        return reader;
    }

    internal void ReturnReader(CborReader reader) => _readersPool.Free(reader);

    internal CborWriter LeaseWriter()
    {
        var writer = _writersPool.Allocate();
        writer.Reset();
        return writer;
    }

    internal void ReturnWriter(CborWriter writer)
    {
        _writersPool.Free(writer);
    }

    private void AssertNotReadOnly()
    {
        if (_isReadOnly) // Optimistic check
        {
            throw new InvalidOperationException("Cannot modify options after it has been used for serialization or deserialization.");
        }

        using var lockScope = _lock.EnterScope();

        if (_isReadOnly)
        {
            throw new InvalidOperationException("Cannot modify options after it has been used for serialization or deserialization.");
        }
    }

    internal void AssertMaxDepth(int currentDepth)
    {
        var maxDepthResolved = MaxDepth == 0 ? DefaultMaxDepth : MaxDepth;

        if (currentDepth > maxDepthResolved)
        {
            throw new CborSerializerException($"Current depth ({currentDepth}) has exceeded maximum allowed depth {maxDepthResolved}.");
        }
    }

    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit. The main
    /// purpose is that limited number of frequently used objects can be kept in the pool for
    /// further recycling.
    ///
    /// Notes:
    /// 1) it is not the goal to keep all returned objects. Pool is not meant for storage. If there
    ///    is no space in the pool, extra returned objects will be dropped.
    ///
    /// 2) it is implied that if object was obtained from a pool, the caller will return it back in
    ///    a relatively short time. Keeping checked out objects for long durations is ok, but
    ///    reduces usefulness of pooling. Just new up your own.
    ///
    /// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice.
    /// Rationale:
    ///    If there is no intent for reusing the object, do not use pool - just use "new".
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    private class ObjectPool<T> where T : class
    {
        // storage for the pool objects.
        private readonly T?[] _items;
        private readonly Func<CborSerializerOptions, T> _factory;
        private readonly CborSerializerOptions _options;

        public ObjectPool(Func<CborSerializerOptions, T> factory, CborSerializerOptions options)
        {
            var size = Environment.ProcessorCount * 4;
            _items = new T[size];
            _factory = factory;
            _options = options;
        }

        public T Allocate()
        {
            // Search strategy is a simple linear probing which is chosen for it cache-friendliness.
            // Note that Free will try to store recycled objects close to the start thus statistically
            // reducing how far we will typically search.

            var items = _items;

            T? inst;

            for (var i = 0; i < items.Length; i++)
            {
                // Note that the read is optimistically not synchronized. That is intentional.
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.

                inst = items[i];

                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i], null, inst))
                    {
                        return inst;
                    }
                }
            }

            _options.MakeReadOnly();
            return _factory(_options);
        }

        /// <summary>
        /// Returns objects to the pool.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search in Allocate.
        /// </remarks>
        public void Free(T obj)
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is null)
                {
                    // Intentionally not using interlocked here.
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i] = obj;
                    break;
                }
            }
        }
    }

}
