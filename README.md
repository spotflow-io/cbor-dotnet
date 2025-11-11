# Spotflow.Cbor

A high-performance .NET library for serializing and deserializing CBOR (Concise Binary Object Representation) data. Built on top of `System.Formats.Cbor`, this library provides a simple, type-safe API similar to `System.Text.Json` for working with CBOR data.

<p align="left">
  <img src="logo.png" alt="Spotflow.Cbor Logo" width="300"/>
</p>


## Features

- Type-safe API with support for generic types.
- Built on top of the official `System.Formats.Cbor` library.
- Support for custom converters.
- Strong nullability support with nullable reference types.
- Support for `required` property modifier.
- Flexible configuration options.
- High-performance serialization and deserialization.


![CI status](https://github.com/spotflow-io/cbor-dotnet/actions/workflows/ci.yml/badge.svg?branch=main)


## Installation

 [![NuGet](https://img.shields.io/nuget/v/Spotflow.Cbor.svg)](https://www.nuget.org/packages/Spotflow.Cbor)

```bash
dotnet add package Spotflow.Cbor
```

## Quick Start

### Basic Serialization

```csharp
using Spotflow.Cbor;

// Serialize an object to CBOR
var person = new Person { Name = "John", Age = 30 };
byte[] cbor = CborSerializer.Serialize(person);

// Deserialize CBOR back to an object
var deserializedPerson = CborSerializer.Deserialize<Person>(cbor);
```

### With Options

```csharp
var options = new CborSerializerOptions
{
    DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = CborNamingPolicy.CamelCase,
};

byte[] cbor = CborSerializer.Serialize(person, options);

var result = CborSerializer.Deserialize<Person>(cbor, options);
```

> [!IMPORTANT]
> **Reuse options instances for optimal performance.** Creating `CborSerializerOptions` is expensive as it initializes object pools and reflection-extracted information are heavily cached for each options instance. Create options once (e.g., as a static readonly field or property) and reuse them across multiple serialization calls.


### Use numeric property names for smaller payloads

To assign numeric property names, use the `CborPropertyAttribute`:

```csharp
public class Person
{
    [CborProperty(NumericName = 1)]
    public string Name { get; set; }
    
    [CborProperty(NumericName = 2)]
    public int Age { get; set; }
}

var options = new CborSerializerOptions
{
    PreferNumericPropertyNames = true // Default
};

var person = new Person { Name = "Alice", Age = 25 };

// Encoded CBOR contains only minimal numeric property names, instead the full text names.

byte[] cbor = CborSerializer.Serialize(person, options); 
```

## Supported .NET Types

### Primitive Types

- **Integers**: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `Int128`, `UInt128`
- **Floating Point**: `Half`, `float`, `double`
- **Boolean**: `bool`
- **String**: `string`
- **Bytes**: `byte[]`, `ReadOnlyMemory<byte>`, `Memory<byte>`.
- **Big Integers**: `BigInteger`

### Date and Time Types

- `DateTime`, `DateTimeOffset` - Supports RFC3339/ISO 8601 text strings and Unix timestamps when reading. When writing, the text strings are used, optionally with a specific CBOR tag.
- `DateOnly`, `TimeOnly` - Reading from .NET-specific text format (e.g., "yyyy-MM-dd" for `DateOnly` and "HH:mm:ss.fffffff" for `TimeOnly`) or from RFC3339/ISO 8601 text strings and Unix timestamps when specific CBOR tags are present when reading. When writing, the .NET-specific text formats are used.
- `TimeSpan` - Reading and writing from a .NET-specific text format ("d.hh:mm:ss.fffffff") or from numbers representing seconds, possibly fractional.

### Other Common Types

- `Guid` - Reading from byte string or .NET specific text formats (when specific CBOR tags are not present). When writing, the byte string format is used.
- `Uri` - Reading and writing absolute and relative URIs.
- `Enum` - By default, enums are read and written as their numeric values. Optionally, their string representations can be used (by adding `CborStringEnumConverter`). When using string representations, custom names can be specified via the `CborStringEnumMemberNameAttribute`. See example below.

### Collection Types

**Lists and Arrays**:
- `List<T>`
- `IList<T>`
- `IReadOnlyList<T>`
- `ICollection<T>`
- `IReadOnlyCollection<T>`
- `IEnumerable<T>`
- `T[]` (arrays)

**Dictionaries**:
- `Dictionary<TKey, TValue>`
- `IDictionary<TKey, TValue>`
- `IReadOnlyDictionary<TKey, TValue>`
- `ConcurrentDictionary<TKey, TValue>`
- `FrozenDictionary<TKey, TValue>`

### Complex Types

- **Custom Classes and Structs** - Serialized as CBOR maps
- **Nested Objects** - Full support for deep object hierarchies
- **Nullable Value Types** - `int?`, `DateTime?`, etc.
- **Nullable Reference Types** - Proper null handling.

### Type Handling Features

- Respect for nullable annotations.
- Deep nesting with configurable max depth.
- Enum serialization as numbers or strings
- Custom property naming with attributes (`CborPropertyAttribute`) and/or `PropertyNamingPolicy` option.
- Optional case-insensitive property name matching.

## Nullability and Required Properties

### The `required` Modifier

Properties marked with the `required` modifier must be present in the CBOR data during deserialization. If a required property is missing, a `CborSerializerException` is thrown:

```csharp
public class Person
{
    public required string Name { get; init; }  // Must be present
    public required int Age { get; init; }      // Must be present
    public string? Nickname { get; init; }      // Optional
}
```
**Important**: The `required` modifier is checked regardless of nullability. Both `required string Name` and `required string? Name` must be present in the CBOR data.

### Reference Types

The `RespectNullableAnnotations` option controls how nullable reference type annotations (`string?` vs `string`) are handled:

**When `RespectNullableAnnotations = false` (default)**:
- Null values are allowed for all reference types, regardless of nullability annotations
- `string` and `string?` are treated identically.
- This matches the default behavior of most serializers.

```csharp
public class Person
{
    public string Name { get; init; }   // Can be null
    public string? Nickname { get; init; } // Can be null
}
```

**When `RespectNullableAnnotations = true`**:
- Non-nullable reference types (`string`) cannot be null.
- Nullable reference types (`string?`) can be null.
- Attempting to deserialize null into a non-nullable reference type throws `CborSerializerException`.
- If a property is not marked as `required`, it is not deserialized if missing, effectively having a null value.

```csharp
public class Person
{
    public string Name1 { get; init; }   // Cannot be deserialized from null, but is not required so the property can effectively have a null value.
    public required string Name2 { get; init; } // Must be present and cannot be deserialized null.
    public string? Name3 { get; init; } // Can be null or missing.
    public required string? Name4 { get; init; } // Must be present, but can be deserialized from null.
}
```

### Value Types

- Null cannot be assigned to non-nullable value types such as `int` or `DateTime`.
- Null can be assigned to nullable value types (e.g., `int?`, `DateTime?`).
- If a non-nullable value type property is missing in the CBOR data, it will receive the default value of that type (e.g., `0` for `int`, `DateTime.MinValue` for `DateTime`). To enforce presence, use the `required` modifier.

```csharp
public class Record
{
    public int Count1 { get; init; }      // Cannot be null but can be missing (default value 0 is assigned).
    public required int Count2 { get; init; } // Must be present and cannot be null.
    public int? Count3 { get; init; } // Can be null or missing.
    public required int? Count4 { get; init; } // Must be present, but can be null.
}
```

### Best Practices

* **Use `required` for mandatory data**: Mark properties as `required` when they must always be present in your data model.
* **Consider `RespectNullableAnnotations = true` for new projects**: This provides stronger type safety and better aligns with C# nullable reference types.
* **Handle missing vs. null**: Remember that "missing" and "null" are different concepts in CBOR. Use `required` to enforce presence, and nullability to control whether null values are allowed.

## CBOR serialization & deserialization

- Definite-length encoding for objects, collections and dictionaries, when possible.
- All numbers (including `BigInteger`) are encoded as a minimal CBOR numeric type.
- CBOR tags are explicitly decoded and provided to converters.
- CBOR self-describing tag (55799) support for both reading and writing.

## Performance

The library is designed with performance in mind:

* **Object Pooling**: `CborReader` and `CborWriter` instances are pooled and reused to minimize allocations.
* **Converter Caching**: Type converters are cached using `ConcurrentDictionary` to avoid repeated reflection and converter resolution:
   - Type-to-converter mappings
   - Property-to-converter mappings
   - Fallback converter cache
   - Nullability type checks
* **Zero reflection**: On subsequent serializations/deserializations for the same types when using the same `CborSerializerOptions` instance, using compiled [System.Linq.Expressions](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions) delegates.
* **Zero-allocation paths**: Where possible, using `Span<T>` and `stackalloc`.
* **TrySerialize API**: Serialize directly into pre-allocated buffers to avoid intermediate allocations.

Overall, the library performs a lot of work during the first serialization/deserialization with a specific `CborSerializerOptions` instance to optimize all subsequent calls with the same options. Therefore, it is not very suitable for scenarios where options can't be reused for multiple calls. 

Library is currently not used source generators, apart compiled [System.Linq.Expressions](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions) delegates.

Example with reusable options:

```csharp
// Configure once, reuse many times
private static  CborSerializerOptions Options { get; } = new()
{
    DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
    MaxDepth = 32
};

// Fast subsequent calls due to cached converters and pooled readers/writers
byte[] cbor1 = CborSerializer.Serialize(obj1, Options);
byte[] cbor2 = CborSerializer.Serialize(obj2, Options);
```

## Configuration Options

### Serialization Behavior

**`DefaultIgnoreCondition`** - Controls when properties are ignored during serialization.

- `CborIgnoreCondition.Never` (default) - Always serialize properties
- `CborIgnoreCondition.WhenWritingNull` - Ignore properties with null values

**`UnmappedMemberHandling`** - Specifies how to handle CBOR properties that don't map to .NET properties during deserialization.

- `CborUnmappedMemberHandling.Skip` (default) - Ignore unmapped properties
- `CborUnmappedMemberHandling.Throw` - Throw an exception when encountering unmapped properties

**`MaxDepth`** - Maximum allowed depth for nested objects and collections. Default is `64` (or `CborSerializerOptions.DefaultMaxDepth`). Set to `0` to use the default.

### Property Naming

**`PropertyNamingPolicy`** - Defines the naming policy for property names. Default is `null` (use property names as-is).

- `CborNamingPolicy.CamelCase` - Convert property names to camelCase

**`PreferNumericPropertyNames`** - When `true` (default), uses numeric property names (defined via `[CborProperty(NumericName = ...)]`) instead of text names (if available) for smaller payload sizes.

**`PropertyNameCaseInsensitive`** - When `true`, property name matching during deserialization is case-insensitive. Default is `false`.

### Nullability

**`RespectNullableAnnotations`** - When `true`, respects nullable reference type annotations (`string?` vs `string`). Default is `false`.

**`HandleUndefinedValuesAsNulls`** - When `true`, treats CBOR undefined values (simple value 23) as null. Default is `false`.

### Type Handling

**`NumberHandling`** - Controls how numbers are read and written.

- `CborNumberHandling.Strict` (default) - Numbers must be encoded as CBOR numbers.
- `CborNumberHandling.AllowReadingFromString` - Allow reading numbers from strings.
- `CborNumberHandling.WriteAsString` - Write numbers as strings.

Flags can be combined: `NumberHandling = CborNumberHandling.AllowReadingFromString | CborNumberHandling.WriteAsString`

**`BooleanHandling`** - Controls how booleans are read during deserialization. This is a flags enum that can be combined.

- `CborBooleanHandling.Strict` (default) - Booleans can only be read from CBOR boolean tokens (major type 7)
- `CborBooleanHandling.AllowReadingFromInteger` - Additionally allows reading booleans from integer tokens (0 for `false`, any other value for `true`)
- `CborBooleanHandling.AllowReadingFromString` - Additionally allows reading booleans from string tokens ("true", "false"), case-insensitive

Flags can be combined: `BooleanHandling = CborBooleanHandling.AllowReadingFromInteger | CborBooleanHandling.AllowReadingFromString`

### CBOR Format

**`ConformanceMode`** - Specifies the CBOR conformance mode.

- `Strict` (default) - Strict RFC 8949 conformance
- Other modes: `Lax`, `Canonical`, `Ctap2Canonical`

**`ConvertIndefiniteLengthEncodings`** - When `true`, converts indefinite-length encodings to definite-length during writing. Default is `false`.

### CBOR Tags

**`WriteSelfDescribeTag`** - When `true`, writes the self-describe CBOR tag (55799) at the start of the output. Default is `false`.

**`WriteDateTimeStringTag`** - When `true`, writes CBOR tag 0 before `DateTime` and `DateTimeOffset` values serialized as RFC3339 strings. Default is `false`.

### Custom Converters

**`Converters`** - A collection of custom `CborConverter` instances to use for serialization/deserialization. Add custom converters to this list to override default behavior for specific types.

## Custom Attributes

### Property Configuration

```csharp
public class Person
{
    [CborProperty(NumericName = 1, TextName = "custom_text_name")]
    public string Name { get; set; }
    
    [CborProperty(NumericName = 2)]
    public int Age { get; set; }
}
```

### Ignoring Properties

```csharp
public class User
{
    public string Username { get; set; }
    
    [CborIgnore]  // Never serialized or deserialized
    public string Password { get; set; }
    
    [CborIgnore(Condition = CborIgnoreCondition.WhenWritingNull)]  // Ignored only when null
    public string? Bio { get; set; }
}
```

### Enum Customization

```csharp
public enum Status
{
    [CborStringEnumMemberName("active")]
    Active,
    
    [CborStringEnumMemberName("inactive")]
    Inactive
}
```

## Custom Converters

Create custom converters by inheriting from `CborConverter<T>`:

```csharp
public class CustomConverter : CborConverter<MyType>
{
    public override MyType Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        // Custom deserialization logic
    }
    
    public override void Write(CborWriter writer, MyType value, CborSerializerOptions options)
    {
        // Custom serialization logic
    }
}

// Register the converter
var options = new CborSerializerOptions();
options.Converters.Add(new CustomConverter());
```

## Error Handling

The library intentionally throws following exceptions:

* `CborSerializerException` - For serialization/deserialization errors with detailed path information.
* `NotSupportedException` - For unsupported types or operations.
* `CborContentException` - Exception thrown by the underlying `CborReader` and `CborWriter` instances.
* `FormatException` - For format-related issues.
* `OverflowException` - For numeric overflows during conversion.

All of these exceptions are intercepted within the library, wrapped into a new exception with additional information (like the current CBOR path) appended to the message.

## Advanced Features

### Working with CborReader/CborWriter

If you need to combine `CborSerializer` with direct `CborReader` or `CborWriter` usage, you can pass your own instances of `CborReader` or `CborWriter` to the serializer. In this case, the instances will not be pooled.

```csharp
var reader = new CborReader(cbor);
var result = CborSerializer.Deserialize<MyType>(reader, options);

var writer = new CborWriter();
var encoded = CborSerializer.Serialize(value, writer, options);
```

### Self-Describe Tag Detection

```csharp
if (CborSerializer.StartsWithSelfDescribeTag(cborData))
{
    // Handle self-described CBOR
}
```

## Maintainers

-   [Tomáš Pajurek](https://github.com/tomas-pajurek) ([Spotflow](https://spotflow.io))

## Contributing

Please read our [Contributing Guidelines](./CONTRIBUTING.md) to learn how you can contribute to this project.

## License

This project is licensed under the [MIT license](./LICENSE.md).
