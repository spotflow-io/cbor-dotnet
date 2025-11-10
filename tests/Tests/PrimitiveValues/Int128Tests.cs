using System.Formats.Cbor;
using System.Numerics;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class Int128Tests
{
    [TestMethod]
    public void Serializing_Int128_Within_Int64_Range_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            Int128Property = 42
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Int128Property");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadInt64().Should().Be(42);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Int128_Beyond_Int64_Range_Should_Write_As_Bignum()
    {
        var model = new TestModel
        {
            Int128Property = Int128.MaxValue
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Int128Property");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.UnsignedBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true).Should().Be(Int128.MaxValue);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Int128_From_Integer_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Int128Property");
        writer.WriteInt64(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.Int128Property.Should().NotBeNull();
        model.Int128Property.Value.Should().Be((Int128) 42);
    }

    [TestMethod]
    public void Deserializing_Int128_From_String_With_AllowReadingFromString_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Int128Property");
        writer.WriteTextString(Int128.MaxValue.ToString());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.Int128Property.Should().NotBeNull();
        model.Int128Property.Value.Should().Be(Int128.MaxValue);
    }

    [TestMethod]
    public void Serializing_Int128_With_WriteAsString_Should_Write_As_String()
    {
        var model = new TestModel
        {
            Int128Property = 12345
        };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.WriteAsString
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Int128Property");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        reader.ReadTextString().Should().Be("12345");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Int128_Min_Value_Should_Roundtrip()
    {
        var model = new TestModel { Int128Property = Int128.MinValue };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Int128Property.Should().NotBeNull();
        deserializedModel.Int128Property.Value.Should().Be(Int128.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Int128_Max_Value_Should_Roundtrip()
    {
        var model = new TestModel { Int128Property = Int128.MaxValue };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Int128Property.Should().NotBeNull();
        deserializedModel.Int128Property.Value.Should().Be(Int128.MaxValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Int128_Zero_Should_Roundtrip()
    {
        var model = new TestModel { Int128Property = 0 };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Int128Property.Should().NotBeNull();
        deserializedModel.Int128Property.Value.Should().Be((Int128) 0);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Negative_Int128_Should_Roundtrip()
    {
        var value = Int128.Parse("-12345678901234567890");
        var model = new TestModel { Int128Property = value };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Int128Property.Should().NotBeNull();
        deserializedModel.Int128Property.Value.Should().Be(value);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Large_Positive_Int128_Should_Roundtrip()
    {
        var value = Int128.Parse("123456789012345678901234567890");
        var model = new TestModel { Int128Property = value };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Int128Property.Should().NotBeNull();
        deserializedModel.Int128Property.Value.Should().Be(value);
    }
}

file class TestModel
{
    public Int128? Int128Property { get; init; }
}
