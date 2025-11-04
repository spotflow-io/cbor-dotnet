using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class ReferenceTypeNullabilityTests
{
    // Required + value is missing

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Required_Non_Nullable_Type_If_Value_Is_Missing_Should_Throw(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor, options);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Missing required property '*TestModelWithRequiredNonNullableProperty.Property'.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Required_Nullable_Type_If_Value_Is_Missing_Should_Throw(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor, options);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Missing required property '*TestModelWithRequiredNullableProperty.Property'.");
    }

    // Required + value is null

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Null_And_If_Annotations_Are_Not_Respected_Should_Yield_Null()
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = false
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Null_And_If_Annotations_Are_Respected_Should_Throw()
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = true
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor, options);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithRequiredNonNullableProperty.Property' at depth 1: Null CBOR value cannot be converted to 'System.String'.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Required_Nullable_Type_If_Value_Is_Null_Should_Yield_Null(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }

    // Required + value is present

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Required_Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().Be("test-value");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Required_Nullable_Type_If_Value_Is_Present_Should_Yield_Value(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().Be("test-value");
    }


    // Non-required + value is missing

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Non_Nullable_Type_If_Value_Is_Missing_Should_Yield_Null(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }


    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Nullable_Type_If_Value_Is_Missing_Should_Yield_Null(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }


    // Non-required + value is null

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Null_And_If_Annotations_Are_Not_Respected_Should_Yield_Null()
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = false
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Null_And_If_Annotations_Are_Respected_Should_Throw()
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = true
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor, options);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithNonNullableProperty.Property' at depth 1: Null CBOR value cannot be converted to 'System.String'.");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Nullable_Type_If_Value_Is_Null_Should_Yield_Null(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().BeNull();
    }

    // Non-required + value is present

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().Be("test-value");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Nullable_Type_If_Value_Is_Present_Should_Yield_Value(bool respectNullableAnnotations)
    {
        var options = new CborSerializerOptions
        {
            RespectNullableAnnotations = respectNullableAnnotations
        };

        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor, options);

        value.Should().NotBeNull();

        value.Property.Should().Be("test-value");
    }
}


file class TestModelWithRequiredNonNullableProperty
{
    public required string Property { get; init; }
}

file class TestModelWithRequiredNullableProperty
{
    public required string? Property { get; init; }
}

file class TestModelWithNonNullableProperty
{
    public string Property { get; init; } = null!;
}

file class TestModelWithNullableProperty
{
    public string? Property { get; init; }
}

