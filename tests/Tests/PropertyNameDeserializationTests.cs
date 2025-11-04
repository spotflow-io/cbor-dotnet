using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class PropertyNameDeserializationTests
{
    [TestMethod]
    public void Deserializing_With_Unknown_Properties_Should_Ignore_Them()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("UnknownProperty");
        rawWriter.WriteTextString("some-value");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.IntegerNullableProperty.Should().BeNull();
        value.StringNullableProperty.Should().BeNull();
    }

    [TestMethod]
    public void Deserializing_With_Numeric_Names_Should_Parse_Values_Correctly()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteInt32(1);
        rawWriter.WriteInt32(100);
        rawWriter.WriteInt32(2);
        rawWriter.WriteTextString("numeric-name-value");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.IntegerNullableProperty.Should().Be(100);
        value.StringNullableProperty.Should().Be("numeric-name-value");
    }

    [TestMethod]
    public void Deserializing_With_Duplicite_Numeric_Names_Should_Throw()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteInt32(1);
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        Action act = () => CborSerializer.Deserialize<InvalidTestModelWithDupliciteNumericNames>(cbor);

        act.Should()
            .Throw<CborModelSerializationException>()
            .WithMessage("Duplicate property numeric name '1' found in '*InvalidTestModelWithDupliciteNumericNames'.");
    }

    [TestMethod]
    public void Deserializing_With_Invalid_Name_Type_Should_Throw()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteBoolean(true);
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        Action act = () => CborSerializer.Deserialize<ValidTestModel>(cbor);

        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Unsupported CBOR key type 'Boolean' for model type '*ValidTestModel'.");

    }

    [TestMethod]
    public void Property_Name_From_Attribute_Should_Be_Used_If_Naming_Policy_Is_Not_Specified()
    {

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
}
