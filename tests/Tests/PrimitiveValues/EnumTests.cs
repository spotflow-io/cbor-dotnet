using System.Formats.Cbor;
using System.Text.Json;
using System.Text.Json.Serialization;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests.PrimitiveValues;

[TestClass]
public class EnumTests
{
    [TestMethod]
    public void Deserializing_Nullable_With_Existing_String_Member_With_String_Converter_Should_Be_Parsed()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty1");
        writer.WriteTextString("Tuesday");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor, options: new() { Converters = { new CborStringEnumConverter() } });

        value.Should().NotBeNull();

        value.NullableProperty1.Should().Be(DayOfWeek.Tuesday);
    }

    [TestMethod]
    public void Deserializing_Nullable_With_Existing_String_Member_Without_String_Converter_Should_Throw()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty1");
        writer.WriteTextString("Tuesday");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
          .Throw<CborSerializerException>()
          .WithMessage("Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'TextString'.\n\n" +
            "Path:\n" +
            "#0: NullableProperty1 {11} (*_TestModel)\n\n" +
            "At: byte 19, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Nullable_With_Non_Existing_String_Member_Should_Throw()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty1");
        writer.WriteTextString("February");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<TestModel>(cbor, options: new() { Converters = { new CborStringEnumConverter() } });
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Invalid text value for enum 'DayOfWeek': 'February'.\n\n" +
                "Path:\n" +
                "#0: NullableProperty1 {11} (*_TestModel)\n\n" +
                "At: byte 28, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Nullable_With_Existing_Numeric_Member_Should_Be_Parsed()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty1");
        writer.WriteUInt32(2);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.NullableProperty1.Should().Be(DayOfWeek.Tuesday);
    }

    [TestMethod]
    public void Deserializing_Nullable_Without_Value_Should_Parse_Null()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var value = CborSerializer.Deserialize<TestModel>(cbor);

        value.Should().NotBeNull();

        value.NullableProperty1.Should().BeNull();
    }

    [TestMethod]
    public void Deserializing_With_Incorrect_Value_Data_Type_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty1");
        writer.WriteBoolean(true);
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor);
        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'UnsignedInteger' or 'NegativeInteger', got 'Boolean'.\n\n" +
                "Path:\n#0: NullableProperty1 {11} (*_TestModel)\n\n" +
                "At: byte 19, depth 1.");
    }

    [TestMethod]
    public void Serializing_Should_Yield_Integer_Value_By_Default()
    {
        var model = new TestModel
        {
            NullableProperty1 = DayOfWeek.Tuesday,
            NullableProperty2 = DateTimeKind.Utc
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NullableProperty1");
        reader.ReadUInt32().Should().Be(2);
        reader.ReadTextString().Should().Be("NullableProperty2");
        reader.ReadUInt32().Should().Be(1);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Should_Yield_String_Value_If_BuiltIn_Converter_Provided()
    {
        var model = new TestModel
        {
            NullableProperty1 = DayOfWeek.Tuesday,
            NullableProperty2 = DateTimeKind.Utc
        };

        var options = new CborSerializerOptions()
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            Converters = { new CborStringEnumConverter() }
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NullableProperty1");
        reader.ReadTextString().Should().Be("Tuesday");
        reader.ReadTextString().Should().Be("NullableProperty2");
        reader.ReadTextString().Should().Be("Utc");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Should_Yield_String_Value_If_BuiltIn_Converter_Provided_For_Specific_Enum_Only()
    {
        var model = new TestModel
        {
            NullableProperty1 = DayOfWeek.Tuesday,
            NullableProperty2 = DateTimeKind.Utc
        };

        var options = new CborSerializerOptions()
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            Converters = { new CborStringEnumConverter<DayOfWeek>() }
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NullableProperty1");
        reader.ReadTextString().Should().Be("Tuesday");
        reader.ReadTextString().Should().Be("NullableProperty2");
        reader.ReadUInt32().Should().Be(1);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Enums_With_All_Possible_Underlying_Types_Should_Work()
    {
        var model = new TestModel
        {
            ByteEnumProperty = ByteEnum.Value,
            SByteEnumProperty1 = SByteEnum.ValuePositive,
            SByteEnumProperty2 = SByteEnum.ValueNegative,
            ShortEnumProperty1 = ShortEnum.ValuePositive,
            ShortEnumProperty2 = ShortEnum.ValueNegative,
            UShortEnumProperty = UShortEnum.Value,
            IntEnumProperty1 = IntEnum.ValuePositive,
            IntEnumProperty2 = IntEnum.ValueNegative,
            UIntEnumProperty = UIntEnum.Value,
            LongEnumProperty1 = LongEnum.ValuePositive,
            LongEnumProperty2 = LongEnum.ValueNegative,
            ULongEnumProperty = ULongEnum.Value
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ByteEnumProperty");
        reader.ReadInt32().Should().Be((byte) ByteEnum.Value);
        reader.ReadTextString().Should().Be("SByteEnumProperty1");
        reader.ReadInt32().Should().Be((sbyte) SByteEnum.ValuePositive);
        reader.ReadTextString().Should().Be("SByteEnumProperty2");
        reader.ReadInt32().Should().Be((sbyte) SByteEnum.ValueNegative);
        reader.ReadTextString().Should().Be("ShortEnumProperty1");
        reader.ReadInt32().Should().Be((short) ShortEnum.ValuePositive);
        reader.ReadTextString().Should().Be("ShortEnumProperty2");
        reader.ReadInt32().Should().Be((short) ShortEnum.ValueNegative);
        reader.ReadTextString().Should().Be("UShortEnumProperty");
        reader.ReadInt32().Should().Be((ushort) UShortEnum.Value);
        reader.ReadTextString().Should().Be("IntEnumProperty1");
        reader.ReadInt32().Should().Be((int) IntEnum.ValuePositive);
        reader.ReadTextString().Should().Be("IntEnumProperty2");
        reader.ReadInt32().Should().Be((int) IntEnum.ValueNegative);
        reader.ReadTextString().Should().Be("UIntEnumProperty");
        reader.ReadUInt32().Should().Be((uint) UIntEnum.Value);
        reader.ReadTextString().Should().Be("LongEnumProperty1");
        reader.ReadInt64().Should().Be((long) LongEnum.ValuePositive);
        reader.ReadTextString().Should().Be("LongEnumProperty2");
        reader.ReadInt64().Should().Be((long) LongEnum.ValueNegative);
        reader.ReadTextString().Should().Be("ULongEnumProperty");
        reader.ReadUInt64().Should().Be((ulong) ULongEnum.Value);
        reader.ReadEndMap();

        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();

        deserializedModel.ByteEnumProperty.Should().Be(ByteEnum.Value);
        deserializedModel.SByteEnumProperty1.Should().Be(SByteEnum.ValuePositive);
        deserializedModel.SByteEnumProperty2.Should().Be(SByteEnum.ValueNegative);
        deserializedModel.ShortEnumProperty2.Should().Be(ShortEnum.ValueNegative);
        deserializedModel.ShortEnumProperty1.Should().Be(ShortEnum.ValuePositive);
        deserializedModel.UShortEnumProperty.Should().Be(UShortEnum.Value);
        deserializedModel.IntEnumProperty1.Should().Be(IntEnum.ValuePositive);
        deserializedModel.IntEnumProperty2.Should().Be(IntEnum.ValueNegative);
        deserializedModel.UIntEnumProperty.Should().Be(UIntEnum.Value);
        deserializedModel.LongEnumProperty1.Should().Be(LongEnum.ValuePositive);
        deserializedModel.LongEnumProperty2.Should().Be(LongEnum.ValueNegative);
        deserializedModel.ULongEnumProperty.Should().Be(ULongEnum.Value);
    }

    [TestMethod]
    public void Deserializing_Enum_With_Undefined_Numeric_Value_For_Should_Work()
    {
        var json = """
        {
            "ByteEnumProperty": 42
        }
        """;

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ByteEnumProperty");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var modelJson = JsonSerializer.Deserialize<TestModel>(json);
        var modelCbor = CborSerializer.Deserialize<TestModel>(cbor);

        modelJson.Should().NotBeNull();
        modelJson.ByteEnumProperty.Should().Be((ByteEnum) 42);

        modelCbor.Should().NotBeNull();
        modelCbor.ByteEnumProperty.Should().Be((ByteEnum) 42);
    }

    [TestMethod]
    public void Deserializing_Enum_With_Undefined_String_Value_With_String_Converter_Should_Throw()
    {
        var json = """
        {
            "ByteEnumProperty": "XXX"
        }
        """;

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ByteEnumProperty");
        writer.WriteTextString("XXX");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var actJson = () => JsonSerializer.Deserialize<TestModel>(json, options: new() { Converters = { new JsonStringEnumConverter() } });
        var actCbor = () => CborSerializer.Deserialize<TestModel>(cbor, new() { Converters = { new CborStringEnumConverter() } });

        actJson.Should()
            .Throw<JsonException>()
            .WithMessage("The JSON value could not be converted to *");

        actCbor.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Invalid text value for enum '*_ByteEnum': 'XXX'.\n\n" +
                "Path:\n" +
                "#0: ByteEnumProperty (*_TestModel)\n\n" +
                "At: byte 22, depth 1.");


    }

    [TestMethod]
    public void Deserializing_Enum_With_Undefined_Numeric_Value_With_String_Converter_Should_Throw()
    {
        var json = """
        {
            "ByteEnumProperty": 42
        }
        """;

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ByteEnumProperty");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var modelJson = JsonSerializer.Deserialize<TestModel>(json, options: new() { Converters = { new JsonStringEnumConverter() } });
        var modelCbor = CborSerializer.Deserialize<TestModel>(cbor, new() { Converters = { new CborStringEnumConverter() } });

        modelJson.Should().NotBeNull();
        modelJson.ByteEnumProperty.Should().Be((ByteEnum) 42);

        modelCbor.Should().NotBeNull();
        modelCbor.ByteEnumProperty.Should().Be((ByteEnum) 42);

    }

    [TestMethod]
    public void Serializing_Enum_With_Custom_Names_Should_Yield_Custom_Names_When_String_Converter_Provided()
    {
        var model = new TestModel
        {
            EnumWithCustomNamesProperty = EnumWithCustomNames.RegularName1
        };
        var options = new CborSerializerOptions()
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            Converters = { new CborStringEnumConverter() }
        };
        var cbor = CborSerializer.Serialize(model, options);
        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("EnumWithCustomNamesProperty");
        reader.ReadTextString().Should().Be("CustomName1");
        reader.ReadEndMap();
    }


    [TestMethod]
    public void Deserializing_Enum_With_Custom_Names_Should_Be_Parsed_When_String_Converter_Provided()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("EnumWithCustomNamesProperty");
        writer.WriteTextString("CustomName1");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var value = CborSerializer.Deserialize<TestModel>(cbor, options: new() { Converters = { new CborStringEnumConverter() } });
        value.Should().NotBeNull();
        value.EnumWithCustomNamesProperty.Should().Be(EnumWithCustomNames.RegularName1);
    }

    [TestMethod]
    public void Deserializing_Enum_With_Custom_Names_Should_Throw_When_String_Converter_Provided_And_Name_Not_Found()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("EnumWithCustomNamesProperty");
        writer.WriteTextString("RegularName1");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        Action actCbor = () => CborSerializer.Deserialize<TestModel>(cbor, options: new() { Converters = { new CborStringEnumConverter() } });

        actCbor.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Invalid text value for enum '*_EnumWithCustomNames': 'RegularName1'.\n\n" +
                "Path:\n" +
                "#0: EnumWithCustomNamesProperty (*_TestModel)\n\n" +
                "At: byte 43, depth 1.");

#if NET9_0_OR_GREATER

        var json = """
        {
            "EnumWithCustomNamesProperty": "RegularName1"
        }
        """;

        Action actJson = () => JsonSerializer.Deserialize<TestModel>(json, options: new() { Converters = { new JsonStringEnumConverter() } });

        actJson.Should()
            .Throw<JsonException>()
            .WithMessage("The JSON value could not be converted to *");
#endif

    }
}

file class TestModel
{
    [CborProperty(NumericName = 11)]
    public DayOfWeek? NullableProperty1 { get; init; }

    [CborProperty(NumericName = 12)]
    public DateTimeKind? NullableProperty2 { get; init; }

    public ByteEnum? ByteEnumProperty { get; init; }
    public SByteEnum? SByteEnumProperty1 { get; init; }
    public SByteEnum? SByteEnumProperty2 { get; init; }
    public ShortEnum? ShortEnumProperty1 { get; init; }
    public ShortEnum? ShortEnumProperty2 { get; init; }
    public UShortEnum? UShortEnumProperty { get; init; }
    public IntEnum? IntEnumProperty1 { get; init; }
    public IntEnum? IntEnumProperty2 { get; init; }
    public UIntEnum? UIntEnumProperty { get; init; }
    public LongEnum? LongEnumProperty1 { get; init; }
    public LongEnum? LongEnumProperty2 { get; init; }
    public ULongEnum? ULongEnumProperty { get; init; }
    public EnumWithCustomNames? EnumWithCustomNamesProperty { get; init; }

}



file enum ByteEnum : byte { Value }
file enum SByteEnum : sbyte { ValueNegative = -1, ValuePositive = 1 }
file enum ShortEnum : short { ValueNegative = -1, ValuePositive = 1 }
file enum UShortEnum : ushort { Value }
file enum IntEnum : int { ValueNegative = -1, ValuePositive = 1 }
file enum UIntEnum : uint { Value }
file enum LongEnum : long { ValueNegative = -1, ValuePositive = 1 }
file enum ULongEnum : ulong { Value }

file enum EnumWithCustomNames
{
    [CborStringEnumMemberName("CustomName1")]
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("CustomName1")]
#endif
    RegularName1,

    RegularName2
}
