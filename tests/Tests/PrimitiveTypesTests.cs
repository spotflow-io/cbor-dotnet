using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class PrimitiveTypesTests
{
    #region String

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
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.StringProperty' (2) at depth 1: Unexpected CBOR data type. Expected 'TextString', got 'Boolean'.");
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
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("StringProperty");
        var propertyValue = reader.ReadTextString();
        propertyValue.Should().Be("test-value");
        reader.ReadEndMap();
    }

    #endregion

    #region Integer

    [TestMethod]
    public void Deserializing_Integer_Nullable_With_Value_Should_Parse_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("IntegerProperty");
        rawWriter.WriteInt32(42);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.IntegerProperty.Should().Be(42);
    }


    [TestMethod]
    public void Deserializing_Integer_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("IntegerProperty");
        rawWriter.WriteTextString("not-an-integer");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.IntegerProperty' (1) at depth 1: Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'TextString'.");
    }

    [TestMethod]
    public void Serializing_Integer_Should_Yield_Value()
    {
        var model = new TestModel
        {
            IntegerProperty = 42
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("IntegerProperty");
        var propertyValue = reader.ReadInt32();
        propertyValue.Should().Be(42);
        reader.ReadEndMap();
    }

    #endregion

    #region Long

    [TestMethod]
    public void Deserializing_Long_Nullable_With_Value_Should_Parse_Value()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("LongProperty");
        rawWriter.WriteInt64(42L);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();
        value.LongProperty.Should().Be(42);
    }

    [TestMethod]
    public void Deserializing_Long_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("LongProperty");
        rawWriter.WriteTextString("not-an-integer");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.LongProperty' (3) at depth 1: Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'TextString'.");
    }

    [TestMethod]
    public void Serializing_Long_Should_Yield_Value()
    {
        var model = new TestModel
        {
            LongProperty = 42L
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("LongProperty");
        var propertyValue = reader.ReadInt64();
        propertyValue.Should().Be(42L);
        reader.ReadEndMap();
    }

    #endregion

    #region ByteArray

    [TestMethod]
    public void Deserializing_ByteArray_Nullable_With_Value_Should_Parse_Value()
    {
        var testBytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ByteArrayProperty");
        rawWriter.WriteByteString(testBytes);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();
        value.ByteArrayProperty.Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_ByteArray_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ByteArrayProperty");
        rawWriter.WriteTextString("not-a-byte-string");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.ByteArrayProperty' (4) at depth 1: Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.");
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
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ReadOnlyMemoryProperty");
        rawWriter.WriteByteString(testBytes);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.ReadOnlyMemoryProperty.Should().NotBeNull();
        value.ReadOnlyMemoryProperty.Value.ToArray().Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_ReadOnlyMemory_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ReadOnlyMemoryProperty");
        rawWriter.WriteTextString("not-a-byte-string");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.ReadOnlyMemoryProperty' (5) at depth 1: Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.");
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
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("MemoryProperty");
        rawWriter.WriteByteString(testBytes);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.MemoryProperty.Should().NotBeNull();
        value.MemoryProperty.Value.ToArray().Should().Equal(testBytes);
    }

    [TestMethod]
    public void Deserializing_Memory_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("MemoryProperty");
        rawWriter.WriteTextString("not-a-byte-string");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.MemoryProperty' (6) at depth 1: Unexpected CBOR data type. Expected 'ByteString', got 'TextString'.");
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

    #region Half

    [TestMethod]
    public void Deserializing_Half_Nullable_With_Value_Should_Parse_Value()
    {
        var testValue = (Half) 3.14f;
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("HalfProperty");
        rawWriter.WriteHalf(testValue);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.HalfProperty.Should().Be(testValue);
    }

    [TestMethod]
    public void Deserializing_Half_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("HalfProperty");
        rawWriter.WriteTextString("not-a-half");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModel.HalfProperty' (7) at depth 1: Unexpected CBOR data type. Expected 'HalfPrecisionFloat', got 'TextString'.");
    }

    [TestMethod]
    public void Serializing_Half_Should_Yield_Value()
    {
        var testValue = (Half) 3.14f;
        var model = new TestModel
        {
            HalfProperty = testValue
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("HalfProperty");
        var propertyValue = reader.ReadHalf();
        propertyValue.Should().Be(testValue);
        reader.ReadEndMap();
    }

    #endregion

    #region Float

    [TestMethod]
    public void Deserializing_Float_Nullable_With_Value_Should_Parse_Value()
    {
        var testValue = 3.14159f;
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("FloatProperty");
        rawWriter.WriteSingle(testValue);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.FloatProperty.Should().Be(testValue);
    }

    [TestMethod]
    public void Deserializing_Float_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("FloatProperty");
        rawWriter.WriteTextString("not-a-float");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborDataSerializationException>()
       .WithMessage("Property '*TestModel.FloatProperty' (8) at depth 1: Unexpected CBOR data type. Expected 'SinglePrecisionFloat', got 'TextString'.");
    }

    [TestMethod]
    public void Serializing_Float_Should_Yield_Value()
    {
        var testValue = 3.14159f;
        var model = new TestModel
        {
            FloatProperty = testValue
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("FloatProperty");
        var propertyValue = reader.ReadSingle();
        propertyValue.Should().Be(testValue);
        reader.ReadEndMap();
    }

    #endregion

    #region Double

    [TestMethod]
    public void Deserializing_Double_Nullable_With_Value_Should_Parse_Value()
    {
        var testValue = 3.141592653589793;
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("DoubleProperty");
        rawWriter.WriteDouble(testValue);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.DoubleProperty.Should().Be(testValue);
    }

    [TestMethod]
    public void Deserializing_Double_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("DoubleProperty");
        rawWriter.WriteTextString("not-a-double");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
          .Throw<CborDataSerializationException>()
               .WithMessage("Property '*TestModel.DoubleProperty' (9) at depth 1: Unexpected CBOR data type. Expected 'DoublePrecisionFloat', got 'TextString'.");
    }

    [TestMethod]
    public void Serializing_Double_Should_Yield_Value()
    {
        var testValue = 3.141592653589793;
        var model = new TestModel
        {
            DoubleProperty = testValue
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        var propertyName = reader.ReadTextString();
        propertyName.Should().Be("DoubleProperty");
        var propertyValue = reader.ReadDouble();
        propertyValue.Should().Be(testValue);
        reader.ReadEndMap();
    }

    #endregion

    #region Boolean

    [TestMethod]
    public void Deserializing_Boolean_Nullable_With_True_Should_Parse_True()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteBoolean(true);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.BooleanProperty.Should().Be(true);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Deserializing_Boolean_Nullable_With_False_Should_Parse_False(bool testValue)
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteBoolean(testValue);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.BooleanProperty.Should().Be(testValue);
    }

    [TestMethod]
    public void Deserializing_Boolean_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteTextString("not-a-boolean");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
       .Throw<CborDataSerializationException>()
          .WithMessage("Property '*TestModel.BooleanProperty' (10) at depth 1: Unexpected CBOR data type. Expected 'Boolean', got 'TextString'.");
    }

    [TestMethod]
    public void Deserializing_Boolean_From_Int_Without_Lax_Deserialization_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteInt32(1);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should().Throw<CborDataSerializationException>()
          .WithMessage("Property '*TestModel.BooleanProperty' (10) at depth 1: Unexpected CBOR data type. Expected 'Boolean', got 'UnsignedInteger'.");
    }

    [TestMethod]
    public void Deserializing_Boolean_From_String_Without_Lax_Deserialization_Should_Throw_Exception()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteTextString("true");
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should().Throw<CborDataSerializationException>()
          .WithMessage("Property '*TestModel.BooleanProperty' (10) at depth 1: Unexpected CBOR data type. Expected 'Boolean', got 'TextString'.");
    }

    [TestMethod]
    [DataRow(1, true)]
    [DataRow(0, false)]
    [DataRow(2, true)]
    [DataRow(-3, true)]
    public void Deserializing_Boolean_From_Int_With_Lax_Deserialization_Should_Succeed(int value, bool expectedValue)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteInt32(value);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor, options: new() { LaxBooleanParsing = true });
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
    public void Deserializing_Boolean_From_String_With_Lax_Deserialization_Should_Succeed(string value, bool expectedValue)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("BooleanProperty");
        rawWriter.WriteTextString(value);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor, options: new() { LaxBooleanParsing = true });
        model.Should().NotBeNull();
        model.BooleanProperty.Should().Be(expectedValue);
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



    #endregion
}

file class TestModel
{
    [CborProperty(NumericName = 1)]
    public int? IntegerProperty { get; init; }

    [CborProperty(NumericName = 2)]
    public string? StringProperty { get; init; }

    [CborProperty(NumericName = 3)]
    public long? LongProperty { get; init; }

    [CborProperty(NumericName = 4)]
    public byte[]? ByteArrayProperty { get; init; }

    [CborProperty(NumericName = 5)]
    public ReadOnlyMemory<byte>? ReadOnlyMemoryProperty { get; init; }

    [CborProperty(NumericName = 6)]
    public Memory<byte>? MemoryProperty { get; init; }

    [CborProperty(NumericName = 7)]
    public Half? HalfProperty { get; init; }

    [CborProperty(NumericName = 8)]
    public float? FloatProperty { get; init; }

    [CborProperty(NumericName = 9)]
    public double? DoubleProperty { get; init; }

    [CborProperty(NumericName = 10)]
    public bool? BooleanProperty { get; init; }

}

