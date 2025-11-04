using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests;

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances

[TestClass]
public class CustomConvertersNullabilityTestsForJson
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Null_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Value_Type_For_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: true, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };


        var result = JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Value_Type_For_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Compatible()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var act = () => JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        act.Should().Throw<JsonException>().WithMessage("The JSON value could not be converted to *TestValueType. Path: $.Property*");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Value_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Value.Should().Be(0);

    }


    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: true, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: false, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(null);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Null_Value_Should_Not_Be_Compatible(bool handleNull)
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var act = () => JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        act.Should().Throw<JsonException>().WithMessage("The JSON value could not be converted to *TestValueType. Path: $.Property*");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Non_Null_Value_Should_Not_Be_Compatible(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var act = () => JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        act.Should().Throw<JsonException>().WithMessage("The JSON value could not be converted to *TestValueType. Path: $.Property*");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Nullable_Value_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestValueType { Value = 42 };

        var converter = new TestNullableValueTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithValueType>(json, options);

        result.Should().NotBeNull();
        result.Property.Value.Should().Be(0);

    }


    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: true, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Null_Value_Without_Null_Handling_Should_Not_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);

        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(null);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Nullable_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithNullableReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_With_Null_Handling_Should_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: true, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_Without_Null_Handling_Without_Nullable_Annotations_Should_Not_Be_Applied()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter }, RespectNullableAnnotations = false };

        var result = JsonSerializer.Deserialize<TestModelWithReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

    }

    [TestMethod]
    public void Converter_For_Reference_Type_For_Property_With_Null_Value_Without_Null_Handling_With_Nullable_Annotations_Should_Not_Be_Compatible()
    {
        var json = """
        {
            "Property": null
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: false, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter }, RespectNullableAnnotations = true };

        var act = () => JsonSerializer.Deserialize<TestModelWithReferenceType>(json, options);

        act.Should().Throw<JsonException>()
            .WithMessage("The property or field 'Property' on type '*TestModelWithReferenceType' doesn't allow setting null values. Consider updating its nullability annotation.*");

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Property_With_Non_Null_Value_Should_Be_Applied(bool handleNull)
    {
        var json = """
        {
            "Property": 43
        }
        """;

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().Be(valueFromConverter);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Converter_For_Reference_Type_For_Property_With_Missing_Value_Should_Not_Be_Applied(bool handleNull)
    {
        var json = "{}";

        var valueFromConverter = new TestReferenceType { Value = 42 };

        var converter = new TestReferenceTypeConverter(handleNull: handleNull, testValue: valueFromConverter);
        var options = new JsonSerializerOptions { Converters = { converter } };

        var result = JsonSerializer.Deserialize<TestModelWithReferenceType>(json, options);

        result.Should().NotBeNull();
        result.Property.Should().BeNull();

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

file class TestValueTypeConverter(bool handleNull, TestValueType testValue) : JsonConverter<TestValueType>
{
    public override bool HandleNull => handleNull;

    public override TestValueType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return testValue;
    }

    public override void Write(Utf8JsonWriter writer, TestValueType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

file class TestNullableValueTypeConverter(bool handleNull, TestValueType? testValue) : JsonConverter<TestValueType?>
{
    public override bool HandleNull => handleNull;
    public override TestValueType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return testValue;
    }
    public override void Write(Utf8JsonWriter writer, TestValueType? value, JsonSerializerOptions options)
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

file class TestReferenceTypeConverter(bool handleNull, TestReferenceType testValue) : JsonConverter<TestReferenceType>
{
    public override bool HandleNull => handleNull;

    public override TestReferenceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return testValue;
    }

    public override void Write(Utf8JsonWriter writer, TestReferenceType value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
