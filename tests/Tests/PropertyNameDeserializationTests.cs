using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class PropertyNameDeserializationTests
{
    [TestMethod]
    public void Deserializing_With_Unknown_Properties_Should_Ignore_Them()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UnknownProperty");
        writer.WriteTextString("some-value");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.IntegerNullableProperty.Should().BeNull();
        value.StringNullableProperty.Should().BeNull();
    }

    [TestMethod]
    public void Deserializing_With_Numeric_Names_Should_Parse_Values_Correctly()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteInt32(1);
        writer.WriteInt32(100);
        writer.WriteInt32(2);
        writer.WriteTextString("numeric-name-value");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.IntegerNullableProperty.Should().Be(100);
        value.StringNullableProperty.Should().Be("numeric-name-value");
    }

    [TestMethod]
    public void Deserializing_With_Duplicite_Numeric_Names_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteInt32(1);
        writer.WriteInt32(42);
        writer.WriteEndMap();
        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<InvalidTestModelWithDupliciteNumericNames>(cbor);

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Duplicate property name 'PropertyB {1}'.\n\n" +
                "At: byte 0, depth 0.");
    }

    [TestMethod]
    public void Deserializing_With_Invalid_Name_Type_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteBoolean(true);
        writer.WriteInt32(42);
        writer.WriteEndMap();
        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<ValidTestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unsupported CBOR type of object property name. Expected 'TextString' or 'UnsignedInteger', got 'Boolean'.\n\n" +
                "At: byte 1, depth 1.");
    }

    [TestMethod]
    public void Property_Name_From_Attribute_Should_Be_Used_If_Naming_Policy_Is_Specified()
    {

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ExplicitNameFromAtt");
        writer.WriteTextString("value1");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<ValidTestModel>(cbor, options: new() { PropertyNamingPolicy = CborNamingPolicy.CamelCase });

        model.Should().NotBeNull();
        model.PropertyWithExplicitTextName.Should().Be("value1");
    }

    [TestMethod]
    public void Property_Name_From_Attribute_Should_Be_Used_If_Naming_Policy_Is_Not_Specified()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ExplicitNameFromAtt");
        writer.WriteTextString("value1");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<ValidTestModel>(cbor);

        model.Should().NotBeNull();
        model.PropertyWithExplicitTextName.Should().Be("value1");
    }
}

file class InvalidTestModelWithDupliciteNumericNames
{
    [CborProperty(NumericName = 1)]
    public int? PropertyA { get; init; }

    [CborProperty(NumericName = 1)]
    public string? PropertyB { get; init; }

}

file class ValidTestModel
{
    [CborProperty(NumericName = 1)]
    public int? IntegerNullableProperty { get; init; }

    [CborProperty(NumericName = 2)]
    public string? StringNullableProperty { get; init; }

    [CborProperty(TextName = "ExplicitNameFromAtt")]
    public string? PropertyWithExplicitTextName { get; init; }
}
