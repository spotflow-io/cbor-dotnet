using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class BooleanTests
{
    [TestMethod]
    public void Deserializing_Boolean_Nullable_With_True_Should_Parse_True()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteBoolean(true);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.BooleanProperty.Should().Be(true);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Deserializing_Boolean_Nullable_With_False_Should_Parse_False(bool testValue)
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteBoolean(testValue);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.BooleanProperty.Should().Be(testValue);
    }

    [TestMethod]
    public void Deserializing_Boolean_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteTextString("not-a-boolean");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'Boolean', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: BooleanProperty {10} (*_TestModel)\n\n" +
                "At: byte 17, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Boolean_From_Int_Without_Lax_Deserialization_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteInt32(1);
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'Boolean', got 'UnsignedInteger'.\n\n" +
              "Path:\n" +
              "#0: BooleanProperty {10} (*_TestModel)\n\n" +
              "At: byte 17, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Boolean_From_String_Without_Lax_Deserialization_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteTextString("true");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'Boolean', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: BooleanProperty {10} (*_TestModel)\n\n" +
                "At: byte 17, depth 1.");
    }

    [TestMethod]
    [DataRow(-3, true)]
    [DataRow(-2, true)]
    [DataRow(-1, true)]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    public void Deserializing_Boolean_From_Int_With_Int_Handling_Should_Succeed(int value, bool expectedValue)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteInt32(value);
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor, options: new() { BooleanHandling = CborBooleanHandling.AllowReadingFromInteger });
        model.Should().NotBeNull();
        model.BooleanProperty.Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow("true", true)]
    [DataRow("false", false)]
    [DataRow("True", true)]
    [DataRow("False", false)]
    [DataRow("TRUE", true)]
    [DataRow("FALSE", false)]
    [DataRow("tRUE", true)]
    [DataRow("fALSE", false)]
    public void Deserializing_Boolean_From_String_With_String_Handling_Deserialization_Should_Succeed(string value, bool expectedValue)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteTextString(value);
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor, options: new() { BooleanHandling = CborBooleanHandling.AllowReadingFromString });
        model.Should().NotBeNull();
        model.BooleanProperty.Should().Be(expectedValue);
    }


    [TestMethod]
    [DataRow("true", 1, true)]
    [DataRow("false", 0, false)]
    [DataRow("True", -1, true)]
    [DataRow("False", 0, false)]
    [DataRow("TRUE", 2, true)]
    [DataRow("FALSE", 0, false)]
    [DataRow("tRUE", -2, true)]
    [DataRow("fALSE", 0, false)]
    public void Deserializing_Boolean_From_String_Or_Int_With_Int_And_String_Handling_Deserialization_Should_Succeed(string stringValue, int intValue, bool expectedValue)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteTextString(stringValue);
        writer.WriteEndMap();

        var cborWithString = writer.Encode();

        writer.Reset();

        writer.WriteStartMap(null);
        writer.WriteTextString("BooleanProperty");
        writer.WriteInt32(intValue);
        writer.WriteEndMap();

        var cborWithInt = writer.Encode();

        var options = new CborSerializerOptions()
        {
            BooleanHandling = CborBooleanHandling.AllowReadingFromInteger | CborBooleanHandling.AllowReadingFromString
        };

        var modelWithString = CborSerializer.Deserialize<TestModel>(cborWithString, options);
        var modelWithInt = CborSerializer.Deserialize<TestModel>(cborWithInt, options);

        modelWithString.Should().NotBeNull();
        modelWithString.BooleanProperty.Should().Be(expectedValue);

        modelWithInt.Should().NotBeNull();
        modelWithInt.BooleanProperty.Should().Be(expectedValue);
    }



    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Serializing_Boolean_Should_Yield_Value(bool testValue)
    {
        var model = new TestModel
        {
            BooleanProperty = testValue
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("BooleanProperty");
        var propertyValue = reader.ReadBoolean();
        propertyValue.Should().Be(testValue);
        reader.ReadEndMap();
    }
}

file class TestModel
{
    [CborProperty(NumericName = 10)]
    public bool? BooleanProperty { get; init; }

}
