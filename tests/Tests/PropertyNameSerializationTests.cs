using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class PropertyNameSerializationTests
{
    [TestMethod]
    public void Numeric_Property_Names_Should_Be_Preferred_By_Default()
    {
        var testModel = new TestModel
        {
            PropertyWithNumericName = "value1",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadUInt64().Should().Be(123);
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Numeric_Property_Names_Should_Not_Be_Used_If_Not_Preffered()
    {
        var testModel = new TestModel
        {
            PropertyWithNumericName = "value1",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull, PreferNumericPropertyNames = false });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("PropertyWithNumericName");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Default_Property_Naming_Policy_Should_Be_Used_If_None_Is_Specified_In_Options()
    {
        var testModel = new TestModel
        {
            PropertyWithExplicitTextName = "value1",
            PropertyWithNumericName = "value2",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new()
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            PreferNumericPropertyNames = false
        });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ExplicitName");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadTextString().Should().Be("PropertyWithNumericName");
        reader.ReadTextString().Should().Be("value2");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Property_Naming_Policy_Specified_In_Options_Should_Be_Used_If_Provided_But_Explicit_Names_Are_Unchanged()
    {
        var testModel = new TestModel
        {
            PropertyWithExplicitTextName = "value1",
            PropertyWithNumericName = "value2",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new()
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            PreferNumericPropertyNames = false,
            PropertyNamingPolicy = CborNamingPolicy.CamelCase
        });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ExplicitName");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadTextString().Should().Be("propertyWithNumericName");
        reader.ReadTextString().Should().Be("value2");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Property_Name_From_Attribute_Should_Be_Used_If_Naming_Policy_Is_Specified()
    {
        var testModel = new TestModel
        {
            PropertyWithExplicitTextName = "value1",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new() { PropertyNamingPolicy = CborNamingPolicy.CamelCase, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ExplicitName");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Property_Name_From_Attribute_Should_Be_Used_If_Naming_Policy_Is_Not_Specified()
    {
        var testModel = new TestModel
        {
            PropertyWithExplicitTextName = "value1",
        };

        var cbor = CborSerializer.Serialize(testModel, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ExplicitName");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }
}

file class TestModel
{
    [CborProperty(TextName = "ExplicitName")]
    public string? PropertyWithExplicitTextName { get; init; }
    [CborProperty(NumericName = 123)]
    public string? PropertyWithNumericName { get; init; }
}
