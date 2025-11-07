using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class MapTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Null_Value_For_Reference_Type_Are_Deserialized(bool respectNullableAnnotations)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var value = CborSerializer.Deserialize<Dictionary<string, string>>(cbor, options: new() { RespectNullableAnnotations = respectNullableAnnotations });

        value.Should().NotBeNull();

        value["Property"].Should().BeNull();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Null_Value_For_Nullable_Value_Type_Are_Deserialized(bool respectNullableAnnotations)
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Property");
        rawWriter.WriteNull();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var value = CborSerializer.Deserialize<Dictionary<string, int?>>(cbor, options: new() { RespectNullableAnnotations = respectNullableAnnotations });

        value.Should().NotBeNull();

        value["Property"].Should().BeNull();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Null_Value_For_Non_Nullable_Value_Type_Should_Throws(bool respectNullableAnnotations)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Property");
        writer.WriteNull();
        writer.WriteEndMap();
        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<Dictionary<string, int>>(cbor, options: new() { RespectNullableAnnotations = respectNullableAnnotations });

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Null CBOR value cannot be converted to 'System.Int32'.\n\n" +
                "Path:\n" +
                "#0: [\"Property\"]\n\n" +
                "At: byte 10, depth 1.");

    }

    [TestMethod]
    [DataRow("Dictionary", typeof(Dictionary<string, TestItem>), typeof(Dictionary<string, TestItem>))]
    [DataRow("IDictionary", typeof(IDictionary<string, TestItem>), typeof(Dictionary<string, TestItem>))]
    [DataRow("IReadOnlyDictionary", typeof(IReadOnlyDictionary<string, TestItem>), typeof(Dictionary<string, TestItem>))]
    public void Map_Of_Objects_Should_Be_Deserialized(string mapPropertyName, Type expectedMapPropertyType, Type expectedMapType)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString(mapPropertyName);
        writer.WriteStartMap(null);
        writer.WriteTextString("key1");
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteTextString("item1");
        writer.WriteEndMap();
        writer.WriteTextString("key2");
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteTextString("item2");
        writer.WriteEndMap();
        writer.WriteEndMap();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestMapModel>(cbor);

        result.Should().NotBeNull();

        var mapProperty = result.GetType().GetProperty(mapPropertyName);

        mapProperty.Should().NotBeNull();

        mapProperty.PropertyType.Should().Be(expectedMapPropertyType);

        var map = (IDictionary<string, TestItem>?) mapProperty.GetValue(result);

        map.Should().BeOfType(expectedMapType);

        map.Should().NotBeNull();

        map.Should().HaveCount(2);
        map["key1"].Value.Should().Be("item1");
        map["key2"].Value.Should().Be("item2");
    }

    [TestMethod]
    public void Map_Should_Be_Serialized_With_Definite_Lenght()
    {
        var testModel = new TestMapModel
        {
            Dictionary = new Dictionary<string, TestItem>
            {
                { "key1", new TestItem { Value = "item1" } },
                { "key2", new TestItem { Value = "item2" } },
            }
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(testModel, options);

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Dictionary");
        reader.ReadStartMap().Should().Be(2); // Definite length map
        reader.ReadTextString().Should().Be("key1");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Value");
        reader.ReadTextString().Should().Be("item1");
        reader.ReadEndMap();
        reader.ReadTextString().Should().Be("key2");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Value");
        reader.ReadTextString().Should().Be("item2");
        reader.ReadEndMap();
        reader.ReadEndMap();

    }

    [TestMethod]
    public void Too_Deep_Nesting_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteStartMap(null);
        writer.WriteTextString("Level1");
        writer.WriteStartMap(null);
        writer.WriteTextString("Level2");
        writer.WriteStartMap(null);
        writer.WriteTextString("Level3");
        writer.WriteInt32(42);
        writer.WriteEndMap();
        writer.WriteEndMap();
        writer.WriteEndMap();
        writer.WriteEndMap();
        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<TooDeepModel>(cbor, new CborSerializerOptions { MaxDepth = 2 });

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Current depth (3) has exceeded maximum allowed depth 2.\n\n" +
                "Path:\n" +
                "#2: [\"Level2\"]\n" +
                "#1: [\"Level1\"]\n" +
                "#0: Value (*_TooDeepModel)\n\n" +
                "At: byte 23, depth 3.");
    }

}

file class TestMapModel
{
    public Dictionary<string, TestItem>? Dictionary { get; init; }
    public IDictionary<string, TestItem>? IDictionary { get; init; }
    public IReadOnlyDictionary<string, TestItem>? IReadOnlyDictionary { get; init; }

}

file class TestItem
{
    public required string Value { get; init; }
}


file class TooDeepModel
{
    public IDictionary<string, IDictionary<string, IDictionary<string, int>>>? Value { get; init; }
}
