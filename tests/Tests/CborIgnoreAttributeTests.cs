using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class CborIgnoreAttributeTests
{
    [TestMethod]
    public void Property_With_CborIgnore_Attribute_Should_Not_Be_Serialized()
    {
        var model = new TestModelWithIgnoredProperty
        {
            IncludedProperty = "included",
            IgnoredProperty = "ignored"
        };

        var cbor = CborSerializer.Serialize(model);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("IncludedProperty");
        reader.ReadTextString().Should().Be("included");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Property_With_CborIgnore_Attribute_Should_Not_Be_Deserialized()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("IncludedProperty");
        writer.WriteTextString("included");
        writer.WriteTextString("IgnoredProperty");
        writer.WriteTextString("ignored");
        writer.WriteEndMap();

        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModelWithIgnoredProperty>(cbor);

        model.Should().NotBeNull();
        model.IncludedProperty.Should().Be("included");
        model.IgnoredProperty.Should().BeNull();
    }

    [TestMethod]
    public void Property_With_CborIgnore_WhenWritingNull_Should_Be_Serialized_When_Not_Null()
    {
        var model = new TestModelWithConditionalIgnore
        {
            Property1 = "value1",
            Property2 = "value2"
        };

        var cbor = CborSerializer.Serialize(model);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Property1");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadTextString().Should().Be("Property2");
        reader.ReadTextString().Should().Be("value2");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Property_With_CborIgnore_WhenWritingNull_Should_Not_Be_Serialized_When_Null()
    {
        var model = new TestModelWithConditionalIgnore
        {
            Property1 = "value1",
            Property2 = null
        };

        var cbor = CborSerializer.Serialize(model);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Property1");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Multiple_Properties_With_CborIgnore_Should_Not_Be_Serialized()
    {
        var model = new TestModelWithMultipleIgnored
        {
            IncludedProperty = "included",
            IgnoredProperty1 = "ignored1",
            IgnoredProperty2 = "ignored2"
        };

        var cbor = CborSerializer.Serialize(model);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("IncludedProperty");
        reader.ReadTextString().Should().Be("included");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void CborIgnore_Should_Work_With_Numeric_Property_Names()
    {
        var model = new TestModelWithNumericNames
        {
            IncludedProperty = "included",
            IgnoredProperty = "ignored"
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = true
        };

        var cbor = CborSerializer.Serialize(model, options);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadInt32().Should().Be(1);
        reader.ReadTextString().Should().Be("included");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void CborIgnore_Should_Work_With_DefaultIgnoreCondition_WhenWritingNull()
    {
        var model = new TestModelWithIgnoredAndNullProperties
        {
            Property1 = "value1",
            Property2 = null,
            IgnoredProperty = "ignored"
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Property1");
        reader.ReadTextString().Should().Be("value1");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void CborIgnore_Attribute_Should_Take_Precedence_Over_DefaultIgnoreCondition()
    {
        var model = new TestModelWithIgnoredProperty
        {
            IncludedProperty = "included",
            IgnoredProperty = "ignored"
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.Never
        };

        var cbor = CborSerializer.Serialize(model, options);
        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("IncludedProperty");
        reader.ReadTextString().Should().Be("included");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Ignored_Property_Should_Not_Prevent_Deserialization_Of_Other_Properties()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("IncludedProperty");
        writer.WriteTextString("included");
        writer.WriteEndMap();

        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModelWithIgnoredProperty>(cbor);

        model.Should().NotBeNull();
        model.IncludedProperty.Should().Be("included");
        model.IgnoredProperty.Should().BeNull();
    }

    [TestMethod]
    public void Ignored_Required_Property_Should_Not_Throw_Missing_Property_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("IncludedProperty");
        writer.WriteTextString("included");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        // This should not throw even though IgnoredRequiredProperty is required
        var model = CborSerializer.Deserialize<TestModelWithIgnoredRequiredProperty>(cbor);

        model.Should().NotBeNull();
        model.IncludedProperty.Should().Be("included");
    }

    [TestMethod]
    public void DefaultIgnoreCondition_Always_Should_Throw_ArgumentException()
    {
        var options = new CborSerializerOptions();

        var act = () => options.DefaultIgnoreCondition = CborIgnoreCondition.Always;

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("'Always' is not valid for 'DefaultIgnoreCondition'. Use the 'CborIgnoreAttribute' attribute to ignore specific properties. (Parameter 'value')");
    }
}

file class TestModelWithIgnoredProperty
{
    public string? IncludedProperty { get; init; }

    [CborIgnore]
    public string? IgnoredProperty { get; init; }
}

file class TestModelWithConditionalIgnore
{
    public string? Property1 { get; init; }

    [CborIgnore(Condition = CborIgnoreCondition.WhenWritingNull)]
    public string? Property2 { get; init; }
}

file class TestModelWithMultipleIgnored
{
    public string? IncludedProperty { get; init; }

    [CborIgnore]
    public string? IgnoredProperty1 { get; init; }

    [CborIgnore]
    public string? IgnoredProperty2 { get; init; }
}

file class TestModelWithNumericNames
{
    [CborProperty(NumericName = 1)]
    public string? IncludedProperty { get; init; }

    [CborIgnore]
    [CborProperty(NumericName = 2)]
    public string? IgnoredProperty { get; init; }
}

file class TestModelWithIgnoredAndNullProperties
{
    public string? Property1 { get; init; }
    public string? Property2 { get; init; }

    [CborIgnore]
    public string? IgnoredProperty { get; init; }
}

file class TestModelWithIgnoredRequiredProperty
{
    public string? IncludedProperty { get; init; }

    [CborIgnore]
    public required string IgnoredRequiredProperty { get; init; }
}

file class TestModelWithoutIgnoreAttribute
{
    public string? Property1 { get; init; }
    public string? Property2 { get; init; }
}
