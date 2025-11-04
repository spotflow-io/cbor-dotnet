using System.Formats.Cbor;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests;


[TestClass]
public class CustomConvertersNullabilityTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Null_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {

        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Value_Type_For_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();


        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: true, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };


        var result = CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Value_Type_For_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Compatible()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var act = () => CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        act.Should().Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithValueType.Property' at depth 1: Null CBOR value cannot be converted to '*TestValueType'.");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Value.Should().Be(0);

    }


    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();


        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: true, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Applied()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: false, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(null);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Null_Value_Should_Not_Be_Compatible(bool handleNull)
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();


        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var act = () => CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        act.Should().Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithValueType.Property' at depth 1: Null CBOR value cannot be converted to '*TestValueType'.");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Non_Null_Value_Should_Not_Be_Compatible(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var act = () => CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithValueType.Property' at depth 1: CBOR value 'UnsignedInteger' could not be converted to '*TestValueType'.");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {

        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithValueType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Value.Should().Be(0);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: true, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Applied()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);

        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(null);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithNullableReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: true, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_Without_Null_Handling_Without_Nullable_Annotations_Should_Not_Be_Applied()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter }, RespectNullableAnnotations = false };

        var result = CborSerializer.Deserialize<TestModelWithReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_Without_Null_Handling_With_Nullable_Annotations_Should_Not_Be_Compatible()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter }, RespectNullableAnnotations = true };

        var act = () => CborSerializer.Deserialize<TestModelWithReferenceType>(cbor, options);

        act.Should()
            .Throw<CborDataSerializationException>()
            .WithMessage("Property '*TestModelWithReferenceType.Property' at depth 1: Null CBOR value cannot be converted to '*TestReferenceType'.");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteInt32(43);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new CborSerializerOptions { Converters = { converter } };

        var result = CborSerializer.Deserialize<TestModelWithReferenceType>(cbor, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Custom_Converter_Defined_On_Type_Should_Be_Discovered_Also_On_Nullable_Of_Same_Type()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteInt32(1);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var options = new CborSerializerOptions();
        var result = CborSerializer.Deserialize<TestModelWithNullableValueTypeWithConverter>(cbor, options);
        result.Should().NotBeNull();
        result.Property.Should().NotBeNull();
        result.Property.Value.Value.Should().Be(TestValueTypeConverter.DefaultValue);
    }

}


readonly file struct TestValueType
{
    public required int Value { get; init; }
};


file class TestModelWithValueType
{
    public TestValueType Property { get; init; }
}

file class TestModelWithNullableValueType
{
    public TestValueType? Property { get; init; }
}

file class TestValueTypeConverter(bool handleNull, TestValueType testValue) : CborConverter<TestValueType>
{
    public TestValueTypeConverter() : this(false, new TestValueType { Value = DefaultValue })
    {
    }

    public const int DefaultValue = 123;

    public override bool HandleNull => handleNull;

    public override TestValueType Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        reader.SkipValue();
        return testValue;
    }

    public override void Write(CborWriter writer, TestValueType value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

file class TestNullableValueTypeConverter(bool handleNull, TestValueType? testValue) : CborConverter<TestValueType?>
{
    public override bool HandleNull => handleNull;
    public override TestValueType? Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        reader.SkipValue();
        return testValue;
    }

    public override void Write(CborWriter writer, TestValueType? value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

file class TestReferenceType
{
    public required int Value { get; init; }
};

file class TestModelWithReferenceType
{
    public TestReferenceType Property { get; init; } = null!;
}

file class TestModelWithNullableReferenceType
{
    public TestReferenceType? Property { get; init; }
}

file class TestModelWithNullableValueTypeWithConverter
{
    [CborConverter<TestValueTypeConverter>]
    public TestValueType? Property { get; init; }

}

file class TestReferenceTypeConverter(bool handleNull, TestReferenceType testValue) : CborConverter<TestReferenceType>
{
    public override bool HandleNull => handleNull;

    public override TestReferenceType Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
    {
        reader.SkipValue();
        return testValue;
    }

    public override void Write(CborWriter writer, TestReferenceType? value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
