using System.Formats.Cbor;
using System.Text.Json;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class ObjectTests
{
    [TestMethod]
    public void Object_Should_Be_Serialized_With_Definite_Lenght_If_DefaultIgnoreCondition_Is_Never()
    {
        var model = new TestModel
        {
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.Never });

        var reader = new CborReader(cbor);

        reader.ReadStartMap().Should().Be(4);
    }


    [TestMethod]
    public void Null_Properties_Should_Be_Serialized_By_Default()
    {
        var model = new TestModel
        {

        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("StringProperty");
        reader.ReadNull();
        reader.ReadTextString().Should().Be("IntegerProperty");
        reader.ReadNull();
        reader.ReadTextString().Should().Be("ObjectProperty");
        reader.ReadNull();
        reader.ReadTextString().Should().Be("ParentClassProperty");
        reader.ReadNull();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Null_Properties_Should_Be_Not_Serialized_When_CborIgnoreCondition_Is_WhenWritingNull()
    {
        var model = new TestModel
        {
            StringProperty = null,
            IntegerProperty = null
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Properties_Should_Be_Deserialized_In_Case_Sensitive_Way_By_Default()
    {
        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = false
        };

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("stringproperty");
        writer.WriteTextString("value1");
        writer.WriteTextString("INTEGERPROPERTY");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.StringProperty.Should().Be(null);
        model.IntegerProperty.Should().Be(null);

    }

    [TestMethod]
    public void Properties_Should_Be_Deserialized_In_Case_Insenstive_Way_If_Specified_Via_Options()
    {
        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("StringProperty");
        writer.WriteTextString("value1");
        writer.WriteTextString("INTEGERPROPERTY");
        writer.WriteInt32(42);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.StringProperty.Should().Be("value1");
        model.IntegerProperty.Should().Be(42);

    }

    [TestMethod]
    public void Unmapped_Properties_Should_Be_Ignored_By_Default()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("StringProperty");
        writer.WriteTextString("value1");
        writer.WriteTextString("UnmappedProperty");
        writer.WriteTextString("value2");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor);
        model.Should().NotBeNull();
        model.StringProperty.Should().Be("value1");
    }

    [TestMethod]
    public void Unmapped_Properties_Should_Throw_If_Disallowed_Via_Options()
    {
        var options = new CborSerializerOptions { UnmappedMemberHandling = CborUnmappedMemberHandling.Disallow };

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("StringProperty");
        writer.WriteTextString("value1");
        writer.WriteTextString("UnmappedProperty");
        writer.WriteTextString("value2");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModel>(cbor, options);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unmapped property 'UnmappedProperty'.\n\n" +
                "At: byte 40, depth 1.");
    }

    [TestMethod]
    public void Property_Of_Parent_Type_Should_Be_Serialized_As_Parent_Type_Even_If_Value_Is_Of_Child_Type()
    {
        var model = new TestModel
        {
            ParentClassProperty = new ChildClass
            {
                Property1 = 10,
                Property2 = "child value"
            }
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ParentClassProperty");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Property1");
        reader.ReadInt32().Should().Be(10);
        reader.ReadEndMap();
        reader.ReadEndMap();

    }

    [TestMethod]
    public void Serializing_Property_Of_Object_Type_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("ObjectProperty");
        writer.WriteStartMap(null);
        writer.WriteTextString("NestedProperty");
        writer.WriteTextString("NestedValue");
        writer.WriteEndMap();
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Cannot serialize or deserialize objects of type 'object'.\n\n" +
            "Path:\n" +
            "#0: ObjectProperty (Tests.*_TestModel)\n\n" +
            "At: byte 16, depth 1.");
    }


    [TestMethod]
    public void Deserializing_Property_Of_Object_Type_Should_Throw()
    {
        var model = new TestModel
        {
            ObjectProperty = new { Property1 = 42 }
        };

        var act = () => CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Cannot serialize or deserialize objects of type 'object'.\n\n" +
                "Path:\n" +
                "#1: ObjectProperty (*_TestModel)\n\n" +
                "At: byte 16, depth 1.");
    }

    [TestMethod]
    public void Deserializing_Object_Of_Value_Type_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("NullableProperty");
        writer.WriteInt32(42);
        writer.WriteTextString("NonNullableProperty");
        writer.WriteInt32(43);

        writer.WriteEndMap();
        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<ValueTypeTestModel>(cbor);
        model.Should().NotBeNull();
        model.NullableProperty.Should().Be(42);
        model.NonNullableProperty.Should().Be(43);

    }

    [TestMethod]
    public void ReadOnly_Properties_Should_Be_Serialized()
    {
        var model = new ClassWithReadOnlyProperty();
        var cbor = CborSerializer.Serialize(model);
        var cborReader = new CborReader(cbor);

        cborReader.ReadStartMap();
        cborReader.ReadTextString().Should().Be("ReadOnlyProperty");
        cborReader.ReadTextString().Should().Be("ReadOnlyValue");
        cborReader.ReadEndMap();

        var json = JsonSerializer.Serialize(model);
        json.Should().Be("""{"ReadOnlyProperty":"ReadOnlyValue"}""");
    }

    [TestMethod]
    public void ReadOnly_Properties_Should_Be_Deserialized_As_Default()
    {
        var cborWriter = new CborWriter();
        cborWriter.WriteStartMap(null);
        cborWriter.WriteTextString("ReadOnlyProperty");
        cborWriter.WriteTextString("NewValue");
        cborWriter.WriteEndMap();
        var cbor = cborWriter.Encode();
        var cborModel = CborSerializer.Deserialize<ClassWithReadOnlyProperty>(cbor);

        cborModel.Should().NotBeNull();
        cborModel.ReadOnlyProperty.Should().Be("ReadOnlyValue");

        var json = """
        {
            "ReadOnlyProperty": "NewValue"
        }
        """;

        var jsonModel = JsonSerializer.Deserialize<ClassWithReadOnlyProperty>(json);

        jsonModel.Should().NotBeNull();
        jsonModel.ReadOnlyProperty.Should().Be("ReadOnlyValue");
    }

    [TestMethod]
    public void Too_Deep_Nesting_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Level2");
        writer.WriteStartMap(null);
        writer.WriteTextString("Level3");
        writer.WriteStartMap(null);
        writer.WriteTextString("Level4");
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteInt32(42);
        writer.WriteEndMap();
        writer.WriteEndMap();
        writer.WriteEndMap();
        writer.WriteEndMap();
        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<TooDeepModelLevel1>(cbor, new CborSerializerOptions { MaxDepth = 2 });

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Current depth (3) has exceeded maximum allowed depth 2.\n\n" +
                "Path:\n" +
                "#2: Level4 (*_TooDeepModelLevel3)\n" +
                "#1: Level3 (*_TooDeepModelLevel2)\n" +
                "#0: Level2 (*_TooDeepModelLevel1)\n\n" +
                "At: byte 24, depth 3."
                );
    }

    [TestMethod]
    public void Property_Of_Type_Type_Must_Not_Be_Deserialized()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TypeProperty");
        writer.WriteTextString("System.String");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        Action act = () => CborSerializer.Deserialize<TestModelWithTypeProperty>(cbor);
        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Serialization or deserialization of 'System.Type' is not supported.\n\n" +
                "Path:\n" +
                "#0: TypeProperty (*_TestModelWithTypeProperty)\n\n" +
                "At: byte 14, depth 1.");
    }

    [TestMethod]
    public void Property_Of_Type_Type_Must_Not_Be_Serialized()
    {
        var model = new TestModelWithTypeProperty
        {
            TypeProperty = typeof(string)
        };

        Action act = () => CborSerializer.Serialize(model);

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("Serialization or deserialization of 'System.Type' is not supported.\n\n" +
                "Path:\n" +
                "#1: TypeProperty (*_TestModelWithTypeProperty)\n\n" +
                "At: byte 1, depth 1.");
    }

}

file class TestModel
{
    public string? StringProperty { get; init; }
    public int? IntegerProperty { get; init; }
    public object? ObjectProperty { get; init; }
    public ParentClass? ParentClassProperty { get; init; }
}

readonly file struct ValueTypeTestModel
{
    public int? NullableProperty { get; init; }
    public int NonNullableProperty { get; init; }

}

file class TestModelWithTypeProperty
{
    public Type? TypeProperty { get; init; }
}

file class ParentClass
{
    public int? Property1 { get; init; }
}

file class ChildClass : ParentClass
{
    public string? Property2 { get; init; }
}


file class ClassWithReadOnlyProperty
{
    public string ReadOnlyProperty => "ReadOnlyValue";

}

file class TooDeepModelLevel1
{
    public TooDeepModelLevel2? Level2 { get; init; }
}

file class TooDeepModelLevel2
{
    public TooDeepModelLevel3? Level3 { get; init; }
}

file class TooDeepModelLevel3
{
    public TooDeepModelLevel4? Level4 { get; init; }
}

file class TooDeepModelLevel4
{
    public int? Value { get; init; }
}
