using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class UndefinedValueTests
{
    [TestMethod]
    public void Only_Undefined_Is_Marked_As_A_Simple_Value_State()
    {
        // Write all simple values and verify only 'undefined' is marked as simple value state

        var writer = new CborWriter();
        writer.WriteStartArray(null);
        writer.WriteSimpleValue(CborSimpleValue.False);
        writer.WriteSimpleValue(CborSimpleValue.True);
        writer.WriteSimpleValue(CborSimpleValue.Null);
        writer.WriteSimpleValue(CborSimpleValue.Undefined);
        writer.WriteEndArray();
        var cbor = writer.Encode();

        var reader = new CborReader(cbor);

        reader.ReadStartArray();

        reader.PeekState().Should().Be(CborReaderState.Boolean);
        reader.ReadSimpleValue().Should().Be(CborSimpleValue.False);

        reader.PeekState().Should().Be(CborReaderState.Boolean);
        reader.ReadSimpleValue().Should().Be(CborSimpleValue.True);

        reader.PeekState().Should().Be(CborReaderState.Null);
        reader.ReadSimpleValue().Should().Be(CborSimpleValue.Null);

        reader.PeekState().Should().Be(CborReaderState.SimpleValue);
        reader.ReadSimpleValue().Should().Be(CborSimpleValue.Undefined);

        reader.ReadEndArray();

    }

    [TestMethod]
    public void Deserializing_Undefined_As_Nullable_Type_Should_Fail_By_Default()
    {

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteSimpleValue(CborSimpleValue.Undefined);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'SimpleValue'.\n\n" +
                "Path:\n" +
                "#0: Value (*_TestModel)\n\nAt: byte 7, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Undefined_As_Nullable_Type_Should_Succeed_When_Option_Is_Set()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteSimpleValue(CborSimpleValue.Undefined);
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var options = new CborSerializerOptions
        {
            HandleUndefinedValuesAsNulls = true
        };
        var model = CborSerializer.Deserialize<TestModel>(cbor, options);
        model.Should().NotBeNull();
        model!.Value.Should().BeNull();
    }
}

file class TestModel
{
    public int? Value { get; set; }
}

