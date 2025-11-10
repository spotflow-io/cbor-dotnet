using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class CollectionTests
{
    [TestMethod]
    [DataRow("List", typeof(List<TestItem>), typeof(List<TestItem>))]
    [DataRow("IList", typeof(IList<TestItem>), typeof(List<TestItem>))]
    [DataRow("IReadOnlyList", typeof(IReadOnlyList<TestItem>), typeof(List<TestItem>))]
    [DataRow("ICollection", typeof(ICollection<TestItem>), typeof(List<TestItem>))]
    [DataRow("IReadOnlyCollection", typeof(IReadOnlyCollection<TestItem>), typeof(List<TestItem>))]
    [DataRow("IEnumerable", typeof(IEnumerable<TestItem>), typeof(List<TestItem>))]
    [DataRow("Array", typeof(TestItem[]), typeof(TestItem[]))]
    public void Collection_Of_Objects_Should_Be_Deserialized(string collectionPropertyName, Type expectedCollectionPropertyType, Type expectedCollectionType)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString(collectionPropertyName);
        writer.WriteStartArray(null);
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteTextString("item1");
        writer.WriteEndMap();
        writer.WriteStartMap(null);
        writer.WriteTextString("Value");
        writer.WriteTextString("item2");
        writer.WriteEndMap();
        writer.WriteEndArray();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().NotBeNull();

        var collectionProperty = result.GetType().GetProperty(collectionPropertyName);

        collectionProperty.Should().NotBeNull();

        collectionProperty.PropertyType.Should().Be(expectedCollectionPropertyType);

        var collection = (IEnumerable<TestItem>?) collectionProperty.GetValue(result);

        collection.Should().BeOfType(expectedCollectionType);

        collection.Should().NotBeNull();

        collection.Should().HaveCount(2);
        collection.ElementAt(0).Value.Should().Be("item1");
        collection.ElementAt(1).Value.Should().Be("item2");

    }

    [TestMethod]
    public void Collection_Should_Be_Serialized_With_Definite_Length()
    {
        var model = new TestModel
        {
            List =
            [
                new TestItem { Value = "item1" },
                new TestItem { Value = "item2" },
            ]
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("List");
        reader.ReadStartArray().Should().Be(2); // Definite length array
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Value");
        reader.ReadTextString().Should().Be("item1");
        reader.ReadEndMap();
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Value");
        reader.ReadTextString().Should().Be("item2");
        reader.ReadEndMap();
        reader.ReadEndArray();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Too_Deep_Nesting_Should_Throw_Exception()
    {
        var writer = new CborWriter();
        writer.WriteStartArray(null);
        writer.WriteStartArray(null);
        writer.WriteEndArray();
        writer.WriteStartArray(null);
        writer.WriteStartArray(null);
        writer.WriteStartArray(null);
        writer.WriteInt32(42);
        writer.WriteEndArray();
        writer.WriteEndArray();
        writer.WriteEndArray();
        writer.WriteEndArray();

        var cbor = writer.Encode();

        Action act = () => CborSerializer.Deserialize<List<List<List<List<int>>>>>(cbor, options: new() { MaxDepth = 2 });

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Current depth (3) has exceeded maximum allowed depth 2.\n\n" +
                "Path:\n" +
                "#2: [0]\n" +
                "#1: [0]\n" +
                "#0: [1]\n\n" +
                "At: byte 5, depth 3.");
    }

}

file class TestModel
{
    public List<TestItem>? List { get; init; }
    public IList<TestItem>? IList { get; init; }
    public IReadOnlyList<TestItem>? IReadOnlyList { get; init; }
    public ICollection<TestItem>? ICollection { get; init; }
    public IReadOnlyCollection<TestItem>? IReadOnlyCollection { get; init; }
    public IEnumerable<TestItem>? IEnumerable { get; init; }
    public TestItem[]? Array { get; init; }
}

file class TestItem
{
    public required string Value { get; init; }
}
