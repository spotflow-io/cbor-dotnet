using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class ValueTypeNullabilityTests
{
    // Required + value is missing

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Missing_Should_Throw()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Missing required property '*TestModelWithRequiredNonNullableProperty.Property'.");
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Missing_Should_Throw()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Missing required property '*TestModelWithRequiredNullableProperty.Property'.");
    }

    // Required + value is null

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Null_Should_Throw()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithRequiredNonNullableProperty.Property' at depth 1: Null CBOR value cannot be converted to 'System.Int32'.");
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Null_Should_Yield_Null()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().BeNull();
    }

    // Required + value is present

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(42);
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Prsent_Should_Yield_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);
        value.Should().NotBeNull();

        value.Property.Should().Be(42);
    }

    // Non-required + value is missing

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Missing_Should_Yield_Default_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);

        value.Should().NotBeNull();
        value.Property.Should().Be(default);
    }

    [TestMethod]
    public void Nullable_Type_When_Value_Is_Missing_Should_Yield_Null()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(null);
    }

    // Non-required + value is null

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Null_Should_Throw()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithNonNullableProperty.Property' at depth 1: Null CBOR value cannot be converted to 'System.Int32'.");
    }

    [TestMethod]
    public void Nullable_Type_If_Value_Is_Null_Should_Yield_Null()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(null);
    }

    // Non-required + value is present

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(42);
    }

    [TestMethod]
    public void Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(42);
    }

}

file class TestModelWithRequiredNonNullableProperty
{
    public required int Property { get; init; }
}

file class TestModelWithRequiredNullableProperty
{
    public required int? Property { get; init; }
}

file class TestModelWithNonNullableProperty
{
    public int Property { get; init; }
}

file class TestModelWithNullableProperty
{
    public int? Property { get; init; }
}
