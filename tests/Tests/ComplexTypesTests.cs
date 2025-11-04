using System.Formats.Cbor;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests;

[TestClass]
public class ComplexTypesTests
{
    [TestMethod]
    public void Nested_Model_Should_Be_Deserialized()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null); // -
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // --
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("Double");
        writer.WriteDouble(3.14);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Key1");
        writer.WriteTextString("Value1");
        writer.WriteTextString("Key2");
        writer.WriteTextString("Value2");
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteEndMap(); // --
        writer.WriteEndMap(); // -

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().NotBeNull();
        result.NestedModel.Should().NotBeNull();
        result.NestedModel.NestedModel.Should().NotBeNull();
        result.NestedModel.NestedModel.Double.Should().Be(3.14);
        result.NestedModel.NestedModel.MapOfStrings.Should().NotBeNull();
        result.NestedModel.NestedModel.MapOfStrings.Should().HaveCount(2);
        result.NestedModel.NestedModel.MapOfStrings.Should().Contain(KeyValuePair.Create("Key1", "Value1"));
        result.NestedModel.NestedModel.MapOfStrings.Should().Contain(KeyValuePair.Create("Key2", "Value2"));

        result.ListOfNestedModels.Should().BeNull();
        result.MapOfNestedModels.Should().BeNull();
        result.ListOfMapsOfNestedModels.Should().BeNull();
        result.EnumKeyedMapOfNestedModels.Should().BeNull();
    }

    [TestMethod]
    public void List_Of_Nested_Models_Should_Be_Deserialized()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null); // -
        writer.WriteTextString("ListOfNestedModels");
        writer.WriteStartArray(null); // --
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(1.23);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("Key0A");
        writer.WriteTextString("Value0A");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(4.56);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("Key1A");
        writer.WriteTextString("Value1A");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteEndArray(); // --
        writer.WriteEndMap(); // -

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().NotBeNull();

        result.ListOfNestedModels.Should().NotBeNull();
        result.ListOfNestedModels.Should().HaveCount(2);
        result.ListOfNestedModels[0].NestedModel.Should().NotBeNull();
        result.ListOfNestedModels[0].NestedModel?.Double.Should().Be(1.23);
        result.ListOfNestedModels[0].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.ListOfNestedModels[0].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.ListOfNestedModels[0].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("Key0A", "Value0A"));
        result.ListOfNestedModels[1].NestedModel.Should().NotBeNull();
        result.ListOfNestedModels[1].NestedModel?.Double.Should().Be(4.56);
        result.ListOfNestedModels[1].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.ListOfNestedModels[1].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.ListOfNestedModels[1].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("Key1A", "Value1A"));

        result.NestedModel.Should().BeNull();
        result.MapOfNestedModels.Should().BeNull();
        result.ListOfMapsOfNestedModels.Should().BeNull();
        result.EnumKeyedMapOfNestedModels.Should().BeNull();
    }

    [TestMethod]
    public void Map_Of_Nested_Models_Should_Be_Deserialized()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null); // -
        writer.WriteTextString("MapOfNestedModels");
        writer.WriteStartMap(null); // --
        writer.WriteTextString("First");
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(7.89);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("KeyA");
        writer.WriteTextString("ValueA");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteTextString("Second");
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(0.12);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("KeyB");
        writer.WriteTextString("ValueB");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteEndMap(); // --
        writer.WriteEndMap(); // -

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().NotBeNull();

        result.MapOfNestedModels.Should().NotBeNull();
        result.MapOfNestedModels.Should().HaveCount(2);
        result.MapOfNestedModels["First"].NestedModel.Should().NotBeNull();
        result.MapOfNestedModels["First"].NestedModel?.Double.Should().Be(7.89);
        result.MapOfNestedModels["First"].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.MapOfNestedModels["First"].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.MapOfNestedModels["First"].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("KeyA", "ValueA"));
        result.MapOfNestedModels["Second"].NestedModel.Should().NotBeNull();
        result.MapOfNestedModels["Second"].NestedModel?.Double.Should().Be(0.12);
        result.MapOfNestedModels["Second"].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.MapOfNestedModels["Second"].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.MapOfNestedModels["Second"].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("KeyB", "ValueB"));

        result.NestedModel.Should().BeNull();
        result.ListOfNestedModels.Should().BeNull();
        result.ListOfMapsOfNestedModels.Should().BeNull();
        result.EnumKeyedMapOfNestedModels.Should().BeNull();

    }

    [TestMethod]
    public void List_Of_Map_Of_Nested_Models_Should_Be_Deserialized()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null); // -
        writer.WriteTextString("ListOfMapsOfNestedModels");
        writer.WriteStartArray(null); // --
        writer.WriteStartMap(null); // --- First map in list
        writer.WriteTextString("MapKey1");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("Double");
        writer.WriteDouble(1.11);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // ------
        writer.WriteTextString("Key1A");
        writer.WriteTextString("Value1A");
        writer.WriteEndMap(); // ------
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteTextString("MapKey2");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("Double");
        writer.WriteDouble(2.22);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // ------
        writer.WriteTextString("Key2A");
        writer.WriteTextString("Value2A");
        writer.WriteEndMap(); // ------
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // --- End first map in list
        writer.WriteStartMap(null); // --- Second map in list
        writer.WriteTextString("MapKey3");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("Double");
        writer.WriteDouble(3.33);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // ------
        writer.WriteTextString("Key3A");
        writer.WriteTextString("Value3A");
        writer.WriteEndMap(); // ------
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // --- End second map in list
        writer.WriteEndArray(); // --
        writer.WriteEndMap(); // -

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().NotBeNull();

        result.ListOfMapsOfNestedModels.Should().NotBeNull();
        result.ListOfMapsOfNestedModels.Should().HaveCount(2);

        // First map in list
        result.ListOfMapsOfNestedModels[0].Should().HaveCount(2);
        result.ListOfMapsOfNestedModels[0]["MapKey1"].NestedModel.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[0]["MapKey1"].NestedModel?.Double.Should().Be(1.11);
        result.ListOfMapsOfNestedModels[0]["MapKey1"].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[0]["MapKey1"].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.ListOfMapsOfNestedModels[0]["MapKey1"].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("Key1A", "Value1A"));
        result.ListOfMapsOfNestedModels[0]["MapKey2"].NestedModel.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[0]["MapKey2"].NestedModel?.Double.Should().Be(2.22);
        result.ListOfMapsOfNestedModels[0]["MapKey2"].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[0]["MapKey2"].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.ListOfMapsOfNestedModels[0]["MapKey2"].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("Key2A", "Value2A"));

        // Second map in list
        result.ListOfMapsOfNestedModels[1].Should().HaveCount(1);
        result.ListOfMapsOfNestedModels[1]["MapKey3"].NestedModel.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[1]["MapKey3"].NestedModel?.Double.Should().Be(3.33);
        result.ListOfMapsOfNestedModels[1]["MapKey3"].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.ListOfMapsOfNestedModels[1]["MapKey3"].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.ListOfMapsOfNestedModels[1]["MapKey3"].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("Key3A", "Value3A"));

        result.NestedModel.Should().BeNull();
        result.ListOfNestedModels.Should().BeNull();
        result.MapOfNestedModels.Should().BeNull();
        result.EnumKeyedMapOfNestedModels.Should().BeNull();
    }

    [TestMethod]
    public void Enum_Keyed_Map_Of_Nested_Models_Should_Be_Deserialized()
    {
        var writer = new CborWriter();

        writer.WriteStartMap(null); // -
        writer.WriteTextString("EnumKeyedMapOfNestedModels");
        writer.WriteStartMap(null); // --
        writer.WriteTextString("Value1");
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(5.67);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("EnumKey1");
        writer.WriteTextString("EnumValue1");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteTextString("Value2");
        writer.WriteStartMap(null); // ---
        writer.WriteTextString("NestedModel");
        writer.WriteStartMap(null); // ----
        writer.WriteTextString("Double");
        writer.WriteDouble(8.90);
        writer.WriteTextString("MapOfStrings");
        writer.WriteStartMap(null); // -----
        writer.WriteTextString("EnumKey2");
        writer.WriteTextString("EnumValue2");
        writer.WriteEndMap(); // -----
        writer.WriteEndMap(); // ----
        writer.WriteEndMap(); // ---
        writer.WriteEndMap(); // --
        writer.WriteEndMap(); // -

        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor, options: new() { Converters = { new CborStringEnumConverter() } });

        result.Should().NotBeNull();

        result.EnumKeyedMapOfNestedModels.Should().NotBeNull();
        result.EnumKeyedMapOfNestedModels.Should().HaveCount(2);
        result.EnumKeyedMapOfNestedModels[TestEnum.Value1].NestedModel.Should().NotBeNull();
        result.EnumKeyedMapOfNestedModels[TestEnum.Value1].NestedModel?.Double.Should().Be(5.67);
        result.EnumKeyedMapOfNestedModels[TestEnum.Value1].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.EnumKeyedMapOfNestedModels[TestEnum.Value1].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.EnumKeyedMapOfNestedModels[TestEnum.Value1].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("EnumKey1", "EnumValue1"));
        result.EnumKeyedMapOfNestedModels[TestEnum.Value2].NestedModel.Should().NotBeNull();
        result.EnumKeyedMapOfNestedModels[TestEnum.Value2].NestedModel?.Double.Should().Be(8.90);
        result.EnumKeyedMapOfNestedModels[TestEnum.Value2].NestedModel?.MapOfStrings.Should().NotBeNull();
        result.EnumKeyedMapOfNestedModels[TestEnum.Value2].NestedModel?.MapOfStrings.Should().HaveCount(1);
        result.EnumKeyedMapOfNestedModels[TestEnum.Value2].NestedModel?.MapOfStrings.Should().Contain(KeyValuePair.Create("EnumKey2", "EnumValue2"));

        result.NestedModel.Should().BeNull();
        result.ListOfNestedModels.Should().BeNull();
        result.MapOfNestedModels.Should().BeNull();
        result.ListOfMapsOfNestedModels.Should().BeNull();
    }

    [TestMethod]
    public void Null_In_Root_Should_Be_Deserialized_As_Null()
    {
        var writer = new CborWriter();
        writer.WriteNull();
        var cbor = writer.Encode();

        var result = CborSerializer.Deserialize<TestModel>(cbor);

        result.Should().BeNull();
    }

    [TestMethod]
    public void Nested_Model_Should_Be_Serialized()
    {
        var model = new TestModel
        {
            NestedModel = new()
            {
                NestedModel = new()
                {
                    Double = 2.34,
                    MapOfStrings = new Dictionary<string, string>
                    {
                        { "A", "B" },
                        { "C", "D" }
                    }
                }
            }
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NestedModel");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NestedModel");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Double");
        reader.ReadDouble().Should().Be(2.34);
        reader.ReadTextString().Should().Be("MapOfStrings");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("A");
        reader.ReadTextString().Should().Be("B");
        reader.ReadTextString().Should().Be("C");
        reader.ReadTextString().Should().Be("D");
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void List_Of_Map_Of_Nested_Models_Should_Be_Serialized()
    {
        var model = new TestModel
        {
            ListOfMapsOfNestedModels =
            [
                new Dictionary<string, TestNestedModelLevel1>
                {
                    {
                        "First", new TestNestedModelLevel1
                        {
                            NestedModel = new TestNestedModelLevel2
                            {
                                Double = 9.87,
                                MapOfStrings = new Dictionary<string, string>
                                {
                                    { "X", "Y" }
                                }
                            }
                        }
                    }
                },
                new Dictionary<string, TestNestedModelLevel1>
                {
                    {
                        "Second", new TestNestedModelLevel1
                        {
                            NestedModel = new TestNestedModelLevel2
                            {
                                Double = 6.54,
                                MapOfStrings = new Dictionary<string, string>
                                {
                                    { "M", "N" }
                                }
                            }
                        }
                    }
                }
            ]
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);

        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("ListOfMapsOfNestedModels");
        reader.ReadStartArray();
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("First");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NestedModel");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Double");
        reader.ReadDouble().Should().Be(9.87);
        reader.ReadTextString().Should().Be("MapOfStrings");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("X");
        reader.ReadTextString().Should().Be("Y");
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Second");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("NestedModel");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("Double");
        reader.ReadDouble().Should().Be(6.54);
        reader.ReadTextString().Should().Be("MapOfStrings");
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("M");
        reader.ReadTextString().Should().Be("N");
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndMap();
        reader.ReadEndArray();

    }

}

file class TestModel
{
    public TestNestedModelLevel1? NestedModel { get; init; }
    public IReadOnlyList<TestNestedModelLevel1>? ListOfNestedModels { get; init; }
    public IReadOnlyDictionary<string, TestNestedModelLevel1>? MapOfNestedModels { get; init; }
    public IReadOnlyList<IReadOnlyDictionary<string, TestNestedModelLevel1>>? ListOfMapsOfNestedModels { get; init; }
    public IReadOnlyDictionary<TestEnum, TestNestedModelLevel1>? EnumKeyedMapOfNestedModels { get; init; }
}

file class TestNestedModelLevel1
{
    public TestNestedModelLevel2? NestedModel { get; init; }
    public IReadOnlyList<TestNestedModelLevel2>? MapOfNestedModels { get; init; }
}

file class TestNestedModelLevel2
{
    public double? Double { get; init; }
    public IReadOnlyDictionary<string, string>? MapOfStrings { get; init; }
}

file enum TestEnum
{
    Value1, Value2
}
