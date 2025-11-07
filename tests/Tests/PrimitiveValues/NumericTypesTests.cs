using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class NumericTypesTests
{
    [TestMethod]
    public void Deserializing_Integers_Should_Succeed()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("Byte");
        writer.WriteInt32(255);
        writer.WriteTextString("SByte");
        writer.WriteInt32(-128);
        writer.WriteTextString("Int16");
        writer.WriteInt32(-32768);
        writer.WriteTextString("UInt16");
        writer.WriteInt32(65535);
        writer.WriteTextString("Int32");
        writer.WriteInt32(42);
        writer.WriteTextString("UInt32");
        writer.WriteInt32(429496725);
        writer.WriteTextString("Int64");
        writer.WriteInt64(-9223372036854775808);
        writer.WriteTextString("UInt64");
        writer.WriteUInt64(18446744073709551615);

        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.Int32.Should().Be(42);
    }

    [TestMethod]
    public void Serializing_Integers_Should_Succeed()
    {
        var model = new TestModel
        {
            Byte = 255,
            SByte = -128,
            Int16 = -32768,
            UInt16 = 65535,
            Int32 = 42,
            UInt32 = 429496725,
            Int64 = -9223372036854775808,
            UInt64 = 18446744073709551615
        };

        var cbor = CborSerializer.Serialize(model, options: new()
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Byte");
        reader.ReadInt32().Should().Be(255);
        reader.ReadTextString().Should().Be("SByte");
        reader.ReadInt32().Should().Be(-128);
        reader.ReadTextString().Should().Be("Int16");
        reader.ReadInt32().Should().Be(-32768);
        reader.ReadTextString().Should().Be("UInt16");
        reader.ReadInt32().Should().Be(65535);
        reader.ReadTextString().Should().Be("Int32");
        reader.ReadInt32().Should().Be(42);
        reader.ReadTextString().Should().Be("UInt32");
        reader.ReadInt32().Should().Be(429496725);
        reader.ReadTextString().Should().Be("Int64");
        reader.ReadInt64().Should().Be(-9223372036854775808);
        reader.ReadTextString().Should().Be("UInt64");
        reader.ReadUInt64().Should().Be(18446744073709551615);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Numbers_With_WriteAsString_Handling_Should_Yield_String()
    {
        var model = new TestModel
        {
            Byte = 255,
            SByte = -128,
            Int16 = -32768,
            UInt16 = 65535,
            Int32 = 42,
            UInt32 = 429496725,
            Int64 = -9223372036854775808,
            UInt64 = 18446744073709551615,
            HalfPrecision = (Half) 1.5f,
            SinglePrecision = 3.14f,
            DoublePrecision = 2.718281828459045
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            NumberHandling = CborNumberHandling.WriteAsString
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Byte");
        reader.ReadTextString().Should().Be("255");
        reader.ReadTextString().Should().Be("SByte");
        reader.ReadTextString().Should().Be("-128");
        reader.ReadTextString().Should().Be("Int16");
        reader.ReadTextString().Should().Be("-32768");
        reader.ReadTextString().Should().Be("UInt16");
        reader.ReadTextString().Should().Be("65535");
        reader.ReadTextString().Should().Be("Int32");
        reader.ReadTextString().Should().Be("42");
        reader.ReadTextString().Should().Be("UInt32");
        reader.ReadTextString().Should().Be("429496725");
        reader.ReadTextString().Should().Be("Int64");
        reader.ReadTextString().Should().Be("-9223372036854775808");
        reader.ReadTextString().Should().Be("UInt64");
        reader.ReadTextString().Should().Be("18446744073709551615");
        reader.ReadTextString().Should().Be("HalfPrecision");
        reader.ReadTextString().Should().Be("1.5");
        reader.ReadTextString().Should().Be("SinglePrecision");
        reader.ReadTextString().Should().Be("3.14");
        reader.ReadTextString().Should().Be("DoublePrecision");
        reader.ReadTextString().Should().Be("2.718281828459045");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Int32_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Int32");
        writer.WriteTextString("not-an-integer");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: Int32 (*_TestModel)\n\n" +
                "At: byte 7, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Int64_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Int64");
        writer.WriteTextString("not-an-integer");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: Int64 (*_TestModel)\n\n" +
                "At: byte 7, depth 1.");
    }


    [TestMethod]
    public void Deserializing_Floating_Points_Should_Succeed()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("HalfPrecision");
        writer.WriteHalf((Half) 3.14f);
        writer.WriteTextString("SinglePrecision");
        writer.WriteSingle(4.14159f);
        writer.WriteTextString("DoublePrecision");
        writer.WriteDouble(5.141592653589793);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.HalfPrecision.Should().Be((Half) 3.14f);
        value.SinglePrecision.Should().Be(4.14159f);
        value.DoublePrecision.Should().Be(5.141592653589793);
    }


    [TestMethod]
    public void Serializing_Floating_Points_Should_Succeed()
    {
        var model = new TestModel
        {
            HalfPrecision = (Half) 3.14f,
            SinglePrecision = 4.14159f,
            DoublePrecision = 5.141592653589793
        };

        var cbor = CborSerializer.Serialize(model, options: new() { PreferNumericPropertyNames = false, DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("HalfPrecision");
        reader.ReadHalf().Should().Be((Half) 3.14f);
        reader.ReadTextString().Should().Be("SinglePrecision");
        reader.ReadSingle().Should().Be(4.14159f);
        reader.ReadTextString().Should().Be("DoublePrecision");
        reader.ReadDouble().Should().Be(5.141592653589793);
        reader.ReadEndMap();
    }


    [TestMethod]
    public void Deserializing_Half_Precision_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("HalfPrecision");
        writer.WriteTextString("not-a-half");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'HalfPrecisionFloat', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: HalfPrecision (*_TestModel)\n\n" +
                "At: byte 15, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Single_Precision_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("SinglePrecision");
        writer.WriteTextString("not-a-float");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'SinglePrecisionFloat' or 'HalfPrecisionFloat', got 'TextString'.\n\n" +
                "Path:\n" +
                "#0: SinglePrecision (*_TestModel)\n\n" +
                "At: byte 17, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Half_Precision_As_Single_Precision_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("SinglePrecision");
        writer.WriteHalf((Half) 1.5f);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.SinglePrecision.Should().Be((float) 1.5f);

    }

    [TestMethod]
    public void Deserialize_Single_Precision_As_Double_Precision_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DoublePrecision");
        writer.WriteSingle(1.5f);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DoublePrecision.Should().Be((double) 1.5f);
    }

    [TestMethod]
    public void Deserializing_Half_Precision_As_Double_Precision_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DoublePrecision");
        writer.WriteHalf((Half) 1.5f);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DoublePrecision.Should().Be((double) 1.5f);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Floating_Point_Literals_Should_Work()
    {
        var posInf = new TestModel
        {
            HalfPrecision = Half.PositiveInfinity,
            SinglePrecision = float.PositiveInfinity,
            DoublePrecision = double.PositiveInfinity
        };

        var negInf = new TestModel
        {
            HalfPrecision = Half.NegativeInfinity,
            SinglePrecision = float.NegativeInfinity,
            DoublePrecision = double.NegativeInfinity
        };

        var nan = new TestModel
        {
            HalfPrecision = Half.NaN,
            SinglePrecision = float.NaN,
            DoublePrecision = double.NaN,
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var posInfCbor = CborSerializer.Serialize(posInf, options);
        var negInfCbor = CborSerializer.Serialize(negInf, options);
        var nanCbor = CborSerializer.Serialize(nan, options);

        var posInfReader = new CborReader(posInfCbor);
        posInfReader.ReadStartMap();
        posInfReader.ReadTextString().Should().Be("HalfPrecision");
        posInfReader.ReadHalf().Should().Be(Half.PositiveInfinity);
        posInfReader.ReadTextString().Should().Be("SinglePrecision");
        posInfReader.ReadSingle().Should().Be(float.PositiveInfinity);
        posInfReader.ReadTextString().Should().Be("DoublePrecision");
        posInfReader.ReadDouble().Should().Be(double.PositiveInfinity);
        posInfReader.ReadEndMap();

        var negInfReader = new CborReader(negInfCbor);
        negInfReader.ReadStartMap();
        negInfReader.ReadTextString().Should().Be("HalfPrecision");
        negInfReader.ReadHalf().Should().Be(Half.NegativeInfinity);
        negInfReader.ReadTextString().Should().Be("SinglePrecision");
        negInfReader.ReadSingle().Should().Be(float.NegativeInfinity);
        negInfReader.ReadTextString().Should().Be("DoublePrecision");
        negInfReader.ReadDouble().Should().Be(double.NegativeInfinity);
        negInfReader.ReadEndMap();

        var nanReader = new CborReader(nanCbor);
        nanReader.ReadStartMap();
        nanReader.ReadTextString().Should().Be("HalfPrecision");
        nanReader.ReadHalf().Should().Be(Half.NaN);
        nanReader.ReadTextString().Should().Be("SinglePrecision");
        nanReader.ReadSingle().Should().Be(float.NaN);
        nanReader.ReadTextString().Should().Be("DoublePrecision");
        nanReader.ReadDouble().Should().Be(double.NaN);
        nanReader.ReadEndMap();

        var deserializedPosInf = CborSerializer.Deserialize<TestModel>(posInfCbor, options);
        var deserializedNegInf = CborSerializer.Deserialize<TestModel>(negInfCbor, options);
        var deserializedNaN = CborSerializer.Deserialize<TestModel>(nanCbor, options);

        deserializedPosInf.Should().NotBeNull();
        deserializedPosInf.HalfPrecision.Should().Be(Half.PositiveInfinity);
        deserializedPosInf.SinglePrecision.Should().Be(float.PositiveInfinity);
        deserializedPosInf.DoublePrecision.Should().Be(double.PositiveInfinity);

        deserializedNegInf.Should().NotBeNull();
        deserializedNegInf.HalfPrecision.Should().Be(Half.NegativeInfinity);
        deserializedNegInf.SinglePrecision.Should().Be(float.NegativeInfinity);
        deserializedNegInf.DoublePrecision.Should().Be(double.NegativeInfinity);

        deserializedNaN.Should().NotBeNull();
        deserializedNaN.HalfPrecision.Should().Be(Half.NaN);
        deserializedNaN.SinglePrecision.Should().Be(float.NaN);
        deserializedNaN.DoublePrecision.Should().Be(double.NaN);
    }


    [TestMethod]
    public void Deserializing_Numbers_From_String_With_AllowReadingFromString_Handling_Should_Parse_Value()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Byte");
        writer.WriteTextString("255");
        writer.WriteTextString("SByte");
        writer.WriteTextString("-128");
        writer.WriteTextString("Int16");
        writer.WriteTextString("-24");
        writer.WriteTextString("UInt16");
        writer.WriteTextString("65535");
        writer.WriteTextString("Int32");
        writer.WriteTextString("-42");
        writer.WriteTextString("UInt32");
        writer.WriteTextString("84");
        writer.WriteTextString("Int64");
        writer.WriteTextString("-84");
        writer.WriteTextString("UInt64");
        writer.WriteTextString("123");
        writer.WriteTextString("SinglePrecision");
        writer.WriteTextString("3.14");
        writer.WriteTextString("DoublePrecision");
        writer.WriteTextString("2.718281828459045");
        writer.WriteTextString("HalfPrecision");
        writer.WriteTextString("1.5");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var options = new CborSerializerOptions
        {
            NumberHandling = CborNumberHandling.AllowReadingFromString
        };
        var value = CborSerializer.Deserialize<TestModel>(cbor, options);
        value.Should().NotBeNull();
        value.Byte.Should().Be(255);
        value.SByte.Should().Be(-128);
        value.Int16.Should().Be(-24);
        value.UInt16.Should().Be(65535);
        value.Int32.Should().Be(-42);
        value.UInt32.Should().Be(84);
        value.Int64.Should().Be(-84);
        value.UInt64.Should().Be(123);
        value.SinglePrecision.Should().Be(3.14f);
        value.DoublePrecision.Should().Be(2.718281828459045);
        value.HalfPrecision.Should().Be((Half) 1.5f);
    }
}


file class TestModel
{
    public byte? Byte { get; init; }
    public sbyte? SByte { get; init; }
    public short? Int16 { get; init; }
    public ushort? UInt16 { get; init; }
    public int? Int32 { get; init; }
    public uint? UInt32 { get; init; }
    public long? Int64 { get; init; }
    public ulong? UInt64 { get; init; }
    public Half? HalfPrecision { get; init; }
    public float? SinglePrecision { get; init; }
    public double? DoublePrecision { get; init; }

}

