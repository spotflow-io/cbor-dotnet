using System.Formats.Cbor;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests.Converters;

[TestClass]
public class CustomConvertersForListTests
{
    [TestMethod]
    public void List_Item_With_Custom_Converter_On_Type_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ListWithItemConverterOnType");
        rawWriter.WriteStartArray(null);
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteUInt32(42);
        rawWriter.WriteEndArray();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.ListWithItemConverterOnType.Should().NotBeNull();
        value.ListWithItemConverterOnType.Should().HaveCount(2);
        value.ListWithItemConverterOnType[0].StringValue.Should().Be("test-value");
        value.ListWithItemConverterOnType[0].IntValue.Should().BeNull();
        value.ListWithItemConverterOnType[1].StringValue.Should().BeNull();
        value.ListWithItemConverterOnType[1].IntValue.Should().Be(42);
    }

    [TestMethod]
    public void List_Item_With_Custom_Converter_On_Property_Should_Be_Applied()
    {
        var rawWriter = new CborWriter();

        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("ListWithItemConverterOnProperty");
        rawWriter.WriteStartArray(null);
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteUInt32(42);
        rawWriter.WriteEndArray();
        rawWriter.WriteEndMap();

        var cbor = rawWriter.Encode();

        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.ListWithItemConverterOnProperty.Should().NotBeNull();
        value.ListWithItemConverterOnProperty.Should().HaveCount(3);
        value.ListWithItemConverterOnProperty[0].StringValue.Should().Be("test-value");
        value.ListWithItemConverterOnProperty[0].IntValue.Should().BeNull();
        value.ListWithItemConverterOnProperty[1].StringValue.Should().BeNull();
        value.ListWithItemConverterOnProperty[1].IntValue.Should().Be(42);
        value.ListWithItemConverterOnProperty[2].Should().BeNull();
    }
}


file class ValidTestModel
{
    public IReadOnlyList<string>? List { get; init; }

    public IReadOnlyList<ItemTestModelWithConverterOnType>? ListWithItemConverterOnType { get; init; }

    [CborConverter<ListConverter<ItemTestModelWithConverterOnType>>]
    public IReadOnlyList<ItemTestModelWithConverterOnType>? ListWithItemConverterOnProperty { get; init; }
}

[CborConverter<ItemTestModelConverter>]
file record ItemTestModelWithConverterOnType(string? StringValue, int? IntValue);

file class ItemTestModelConverter : CborConverter<ItemTestModelWithConverterOnType>
{
    public override ItemTestModelWithConverterOnType Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        if (state is CborReaderState.TextString)
        {
            return new(reader.ReadTextString(), null);
        }
        else if (state is CborReaderState.UnsignedInteger)
        {
            return new(null, (int) reader.ReadUInt32());
        }
        else
        {
            throw UnexpectedDataType(expected: [CborReaderState.TextString, CborReaderState.UnsignedInteger], state);
        }
    }

    public override void Write(CborWriter writer, ItemTestModelWithConverterOnType? value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}


file class ListConverter<T> : CborConverter<IReadOnlyList<T>>
{
    public override IReadOnlyList<T>? Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        reader.ReadStartArray();

        var list = new List<T>();

        while (reader.PeekState() != CborReaderState.EndArray)
        {
            var item = CborSerializer.Deserialize<T>(reader, options);

            if (item is null)
            {
                throw new InvalidOperationException("List item cannot be null");
            }

            list.Add(item);
        }

        reader.ReadEndArray();

        list.Add(default!);

        return list;
    }

    public override void Write(CborWriter writer, IReadOnlyList<T>? value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
