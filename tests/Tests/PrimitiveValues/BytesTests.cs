using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class BytesTests
{
    #region ByteArray

    [TestMethod]
    public void Deserializing_ByteArray_Nullable_With_Value_Should_Parse_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("ByteArrayProperty");
        writer.WriteByteString(testBytes);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();
        value.ByteArrayProperty.Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_ByteArray_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ByteArrayProperty");
        writer.WriteTextString("not-a-byte-string");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: ByteArrayProperty {4} (*_TestModel)\n\n" +
                "At: byte 19, depth 1.");
    }

    [TestMethod]
    public void Serializing_ByteArray_Should_Yield_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var model = new TestModel
        {
            ByteArrayProperty = testBytes
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("ByteArrayProperty");
        var propertyValue = reader.ReadByteString();
        propertyValue.Should().Equal(testBytes);
        reader.ReadEndMap();
    }

    #endregion

    #region ReadOnlyMemory<byte>

    [TestMethod]
    public void Deserializing_ReadOnlyMemory_Nullable_With_Value_Should_Parse_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("ReadOnlyMemoryProperty");
        writer.WriteByteString(testBytes);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.ReadOnlyMemoryProperty.Should().NotBeNull();
        value.ReadOnlyMemoryProperty.Value.ToArray().Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_ReadOnlyMemory_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ReadOnlyMemoryProperty");
        writer.WriteTextString("not-a-byte-string");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: ReadOnlyMemoryProperty {5} (*_TestModel)\n\n" +
                "At: byte 24, depth 1.");
    }

    [TestMethod]
    public void Serializing_ReadOnlyMemory_Should_Yield_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var model = new TestModel
        {
            ReadOnlyMemoryProperty = new ReadOnlyMemory<byte>(testBytes)
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("ReadOnlyMemoryProperty");
        var propertyValue = reader.ReadByteString();
        propertyValue.Should().Equal(testBytes);
        reader.ReadEndMap();
    }

    #endregion

    #region Memory<byte>

    [TestMethod]
    public void Deserializing_Memory_Nullable_With_Value_Should_Parse_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("MemoryProperty");
        writer.WriteByteString(testBytes);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.MemoryProperty.Should().NotBeNull();
        value.MemoryProperty.Value.ToArray().Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_Memory_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("MemoryProperty");
        writer.WriteTextString("not-a-byte-string");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.\n\n" +
            "Path:\n" +
            "#0: MemoryProperty {6} (*_TestModel)\n\n" +
            "At: byte 16, depth 1.");
    }

    [TestMethod]
    public void Serializing_Memory_Should_Yield_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var model = new TestModel
        {
            MemoryProperty = new Memory<byte>(testBytes)
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("MemoryProperty");
        var propertyValue = reader.ReadByteString();
        propertyValue.Should().Equal(testBytes);
        reader.ReadEndMap();
    }

    #endregion
}

file class TestModel
{

    [CborProperty(NumericName = 4)]
    public byte[]? ByteArrayProperty { get; init; }

    [CborProperty(NumericName = 5)]
    public ReadOnlyMemory<byte>? ReadOnlyMemoryProperty { get; init; }

    [CborProperty(NumericName = 6)]
    public Memory<byte>? MemoryProperty { get; init; }
}

