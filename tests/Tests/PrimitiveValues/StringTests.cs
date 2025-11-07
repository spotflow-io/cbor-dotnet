using System.Formats.Cbor;
using System.Text.Json;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class StringTests
{
    [TestMethod]
    public void Deserializing_String_With_Value_Should_Parse_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("StringProperty");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.StringProperty.Should().Be("test-value");
    }

    [TestMethod]
    public void Deserializing_String_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("StringProperty");
        rawWriter.WriteBoolean(true);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'TextString', got 'Boolean'.\n\n" +
                "Path:\n" +
                "#0: StringProperty {2} (*_TestModel)\n\n" +
                "At: byte 16, depth 1.");
    }

    [TestMethod]
    public void Serializing_String_Should_Yield_Value()
    {
        var model = new TestModel
        {
            StringProperty = "test-value"
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("StringProperty");
        reader.ReadTextString().Should().Be("test-value");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Number_Into_String_Should_Throw()
    {
        var json = """
            {
                "StringProperty": 12345
            }
            """;

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("StringProperty");
        writer.WriteInt32(12345);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var actJson = () => JsonSerializer.Deserialize<TestModel>(json);
        var actCbor = () => CborSerializer.Deserialize<TestModel>(cbor);


        actJson.Should()
            .Throw<JsonException>()
            .WithMessage("The JSON value could not be converted to System.String. Path: $.StringProperty *");

        actCbor.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'TextString', got 'UnsignedInteger'.\n\n" +
                "Path:\n" +
                "#0: StringProperty {2} (*_TestModel)\n\n" +
                "At: byte 16, depth 1.");

    }

}


file class TestModel
{
    [CborProperty(NumericName = 2)]
    public string? StringProperty { get; init; }
}
