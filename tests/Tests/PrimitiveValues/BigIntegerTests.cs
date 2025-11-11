using System.Formats.Cbor;
using System.Numerics;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class BigIntegerTests
{
    [TestMethod]
    public void Serializing_BigInteger_Within_Int64_Range_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            BigIntegerProperty = 42
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadInt64().Should().Be(42);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Negative_Within_Int64_Range_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            BigIntegerProperty = -42
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.NegativeInteger);
        reader.ReadInt64().Should().Be(-42);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Beyond_Int64_Range_Should_Write_As_Bignum()
    {
        var value = BigInteger.Parse("123456789012345678901234567890");
        var model = new TestModel
        {
            BigIntegerProperty = value
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.UnsignedBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true).Should().Be(value);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Negative_Beyond_Int64_Range_Should_Write_As_NegativeBignum()
    {
        var value = BigInteger.Parse("-123456789012345678901234567890");
        var model = new TestModel
        {
            BigIntegerProperty = value
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.NegativeBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        var n = new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true);
        (-1 - n).Should().Be(value);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_BigInteger_From_Integer_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BigIntegerProperty");
        writer.WriteInt64(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.BigIntegerProperty.Should().NotBeNull();
        model.BigIntegerProperty.Value.Should().Be((BigInteger) 42);
    }

    [TestMethod]
    public void Deserializing_BigInteger_From_NegativeInteger_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BigIntegerProperty");
        writer.WriteInt64(-42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.BigIntegerProperty.Should().NotBeNull();
        model.BigIntegerProperty.Value.Should().Be((BigInteger) (-42));
    }

    [TestMethod]
    public void Deserializing_BigInteger_From_UnsignedBignum_Should_Parse()
    {
        var value = BigInteger.Parse("123456789012345678901234567890");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BigIntegerProperty");
        writer.WriteTag(CborTag.UnsignedBigNum);
        var bytesNeeded = value.GetByteCount(isUnsigned: true);
        Span<byte> bytes = stackalloc byte[bytesNeeded];
        value.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: true);
        writer.WriteByteString(bytes);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.BigIntegerProperty.Should().NotBeNull();
        model.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Deserializing_BigInteger_From_NegativeBignum_Should_Parse()
    {
        var value = BigInteger.Parse("-123456789012345678901234567890");
        var n = -1 - value;
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BigIntegerProperty");
        writer.WriteTag(CborTag.NegativeBigNum);
        var bytesNeeded = n.GetByteCount(isUnsigned: true);
        Span<byte> bytes = stackalloc byte[bytesNeeded];
        n.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: true);
        writer.WriteByteString(bytes);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.BigIntegerProperty.Should().NotBeNull();
        model.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Deserializing_BigInteger_From_String_With_AllowReadingFromString_Should_Parse()
    {
        var value = BigInteger.Parse("999999999999999999999999999999999999999");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("BigIntegerProperty");
        writer.WriteTextString(value.ToString());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.BigIntegerProperty.Should().NotBeNull();
        model.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Serializing_BigInteger_With_WriteAsString_Should_Write_As_String()
    {
        var model = new TestModel
        {
            BigIntegerProperty = 12345
        };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.WriteAsString
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        reader.ReadTextString().Should().Be("12345");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_BigInteger_Zero_Should_Roundtrip()
    {
        var model = new TestModel { BigIntegerProperty = BigInteger.Zero };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(BigInteger.Zero);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_BigInteger_One_Should_Roundtrip()
    {
        var model = new TestModel { BigIntegerProperty = BigInteger.One };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(BigInteger.One);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_BigInteger_MinusOne_Should_Roundtrip()
    {
        var model = new TestModel { BigIntegerProperty = BigInteger.MinusOne };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(BigInteger.MinusOne);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Large_Positive_BigInteger_Should_Roundtrip()
    {
        var value = BigInteger.Parse("123456789012345678901234567890123456789012345678901234567890");
        var model = new TestModel { BigIntegerProperty = value };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Large_Negative_BigInteger_Should_Roundtrip()
    {
        var value = BigInteger.Parse("-123456789012345678901234567890123456789012345678901234567890");
        var model = new TestModel { BigIntegerProperty = value };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_BigInteger_With_WriteAsString_Should_Roundtrip()
    {
        var value = BigInteger.Parse("999888777666555444333222111000");
        var model = new TestModel { BigIntegerProperty = value };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.WriteAsString | CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Should().NotBeNull();
        deserializedModel.BigIntegerProperty.Value.Should().Be(value);
    }

    [TestMethod]
    public void Serializing_BigInteger_Long_MaxValue_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            BigIntegerProperty = long.MaxValue
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadInt64().Should().Be(long.MaxValue);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Long_MinValue_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            BigIntegerProperty = long.MinValue
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.NegativeInteger);
        reader.ReadInt64().Should().Be(long.MinValue);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Beyond_Long_MaxValue_Should_Write_As_Bignum()
    {
        // Use a value beyond UInt64.MaxValue to ensure bignum encoding
        var value = (BigInteger) ulong.MaxValue + 1;
        var model = new TestModel
        {
            BigIntegerProperty = value
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.UnsignedBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true).Should().Be(value);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Between_Long_MaxValue_And_UInt64_MaxValue_Should_Write_As_UnsignedInteger()
    {
        // Value between long.MaxValue and ulong.MaxValue should be written as UInt64
        var value = (BigInteger) long.MaxValue + 1;
        var model = new TestModel
        {
            BigIntegerProperty = value
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadUInt64().Should().Be((ulong) value);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_UInt64_MaxValue_Should_Write_As_UnsignedInteger()
    {
        var model = new TestModel
        {
            BigIntegerProperty = ulong.MaxValue
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadUInt64().Should().Be(ulong.MaxValue);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_BigInteger_Beyond_Long_MinValue_Should_Write_As_NegativeBignum()
    {
        var value = (BigInteger) long.MinValue - 1;
        var model = new TestModel
        {
            BigIntegerProperty = value
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("BigIntegerProperty");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.NegativeBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        var n = new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true);
        (-1 - n).Should().Be(value);
        reader.ReadEndMap();
    }
}

file class TestModel
{
    public BigInteger? BigIntegerProperty { get; init; }
}
