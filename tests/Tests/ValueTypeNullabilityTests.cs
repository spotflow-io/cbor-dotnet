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
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Required properties are missing: 'Property'.\n\n" +
                "At: byte 1, depth 1.");
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Missing_Should_Throw()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);

        action.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Required properties are missing: 'Property'.\n\n" +
                "At: byte 1, depth 1.");
    }

    // Required + value is null

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Null_Should_Throw()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Null CBOR value cannot be converted to 'System.Int32'.\n\n" +
                "Path:\n" +
                "#0: Property (*_TestModelWithRequiredNonNullableProperty)\n\n" +
                "At: byte 10, depth 1.");
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Null_Should_Yield_Null()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().BeNull();
    }

    // Required + value is present

    [TestMethod]
    public void Required_Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNonNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(42);
    }

    [TestMethod]
    public void Required_Nullable_Type_If_Value_Is_Prsent_Should_Yield_Value()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithRequiredNullableProperty>(cbor);
        value.Should().NotBeNull();

        value.Property.Should().Be(42);
    }

    // Non-required + value is missing

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Missing_Should_Yield_Default_Value()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);

        value.Should().NotBeNull();
        value.Property.Should().Be(default);
    }

    [TestMethod]
    public void Nullable_Type_When_Value_Is_Missing_Should_Yield_Null()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(null);
    }

    // Non-required + value is null

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Null_Should_Throw()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var action = () => CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);

        action.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Null CBOR value cannot be converted to 'System.Int32'.\n\n" +
                "Path:\n" +
                "#0: Property (*_TestModelWithNonNullableProperty)\n\n" +
                "At: byte 10, depth 1.");
    }

    [TestMethod]
    public void Nullable_Type_If_Value_Is_Null_Should_Yield_Null()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(null);
    }

    // Non-required + value is present

    [TestMethod]
    public void Non_Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModelWithNonNullableProperty>(cbor);
        value.Should().NotBeNull();
        value.Property.Should().Be(42);
    }

    [TestMethod]
    public void Nullable_Type_If_Value_Is_Present_Should_Yield_Value()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

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
