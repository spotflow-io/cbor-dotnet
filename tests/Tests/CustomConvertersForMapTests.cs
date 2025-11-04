using System.Formats.Cbor;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests;

[TestClass]
public class CustomConvertersForMapTests
{
    [TestMethod]
    public void Property_With_Custom_Converter_Should_Be_Parsed()
    {
        var rawWriter = new CborWriter();
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("Map");
        rawWriter.WriteStartMap(null);
        rawWriter.WriteTextString("first");
        rawWriter.WriteTextString("test-value");
        rawWriter.WriteTextString("second");
        rawWriter.WriteUInt32(42);
        rawWriter.WriteEndMap();
        rawWriter.WriteEndMap();
        var cbor = rawWriter.Encode();
        var value = CborSerializer.Deserialize<ValidTestModel>(cbor);

        value.Should().NotBeNull();

        value.Map.Should().NotBeNull();
        value.Map.Should().HaveCount(2);
        value.Map["first"].StringValue.Should().Be("test-value");
        value.Map["first"].IntValue.Should().BeNull();
        value.Map["second"].StringValue.Should().BeNull();
        value.Map["second"].IntValue.Should().Be(42);

    }
}


file class ValidTestModel
{

    [CborProperty(NumericName = 1)]
    public IReadOnlyDictionary<string, ItemTestModel>? Map { get; init; }
}

[CborConverter<ItemTestModelConverter>]
file record ItemTestModel(string? StringValue, int? IntValue);

file class ItemTestModelConverter : CborConverter<ItemTestModel>
{
    public override ItemTestModel Read(CborReader reader, Type typeToConvert, CborSerializerOptions options)
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

    public override void Write(CborWriter writer, ItemTestModel? value, CborSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
