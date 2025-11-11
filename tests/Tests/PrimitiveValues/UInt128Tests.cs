using System.Formats.Cbor;
using System.Numerics;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class UInt128Tests
{
    [TestMethod]
    public void Serializing_UInt128_Within_UInt64_Range_Should_Write_As_Integer()
    {
        var model = new TestModel
        {
            UInt128Property = 42
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UInt128Property");
        reader.PeekState().Should().Be(CborReaderState.UnsignedInteger);
        reader.ReadUInt64().Should().Be(42);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_UInt128_Beyond_UInt64_Range_Should_Write_As_Bignum()
    {
        var model = new TestModel
        {
            UInt128Property = UInt128.MaxValue
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UInt128Property");
        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.ReadTag().Should().Be(CborTag.UnsignedBigNum);
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        new BigInteger(reader.ReadByteString(), isUnsigned: true, isBigEndian: true).Should().Be(UInt128.MaxValue);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_UInt128_From_Integer_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UInt128Property");
        writer.WriteUInt64(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UInt128Property.Should().NotBeNull();
        model.UInt128Property.Value.Should().Be((UInt128) 42);
    }

    [TestMethod]
    public void Deserializing_UInt128_From_String_With_AllowReadingFromString_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UInt128Property");
        writer.WriteTextString(UInt128.MaxValue.ToString());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.UInt128Property.Should().NotBeNull();
        model.UInt128Property.Value.Should().Be(UInt128.MaxValue);
    }

    [TestMethod]
    public void Serializing_UInt128_With_WriteAsString_Should_Write_As_String()
    {
        var model = new TestModel
        {
            UInt128Property = 12345
        };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.WriteAsString
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UInt128Property");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        reader.ReadTextString().Should().Be("12345");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_UInt128_Min_Value_Should_Roundtrip()
    {
        var model = new TestModel { UInt128Property = UInt128.MinValue };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UInt128Property.Should().NotBeNull();
        deserializedModel.UInt128Property.Value.Should().Be(UInt128.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_UInt128_Max_Value_Should_Roundtrip()
    {
        var model = new TestModel { UInt128Property = UInt128.MaxValue };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UInt128Property.Should().NotBeNull();
        deserializedModel.UInt128Property.Value.Should().Be(UInt128.MaxValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_UInt128_Zero_Should_Roundtrip()
    {
        var model = new TestModel { UInt128Property = 0 };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UInt128Property.Should().NotBeNull();
        deserializedModel.UInt128Property.Value.Should().Be((UInt128) 0);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Large_UInt128_Should_Roundtrip()
    {
        var value = UInt128.Parse("123456789012345678901234567890");
        var model = new TestModel { UInt128Property = value };

        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UInt128Property.Should().NotBeNull();
        deserializedModel.UInt128Property.Value.Should().Be(value);
    }

    [TestMethod]
    public void Deserializing_UInt128_From_Negative_Integer_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UInt128Property");
        writer.WriteInt64(-42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<OverflowException>()
            .WithMessage("Arithmetic operation resulted in an overflow.\n\n" +
                "Path:\n" +
                "#0: UInt128Property (*_TestModel)\n\n" +
                "At: byte 19, depth 1.");
    }
}

file class TestModel
{
    public UInt128? UInt128Property { get; init; }
}
