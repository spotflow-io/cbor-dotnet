using System.Formats.Cbor;

using Spotflow.Cbor;
using Spotflow.Cbor.Converters;

namespace Tests;

[TestClass]
public class TagsTests
{
    [TestMethod]
    public void Tags_Should_Not_Break_Default_Converters()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("IntegerProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt32(1762784640);
        writer.WriteEndMap();

        var cborData = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cborData);

        model.Should().NotBeNull();
        model.IntegerProperty.Should().Be(1762784640);
    }

    [TestMethod]
    public void Tags_Should_Be_Available_To_Converters_On_Read()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("CustomTypeProperty");
        writer.WriteTag(CborTag.Uri);
        writer.WriteTextString("https://example.com");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.CustomTypeProperty.Should().NotBeNull();
        model.CustomTypeProperty.Tag.Should().Be(CborTag.Uri);
        model.CustomTypeProperty.Value.Should().Be("https://example.com");
    }

    [TestMethod]
    public void Tags_Should_Can_Be_Written_By_Converters_On_Write()
    {
        var model = new TestModel
        {
            CustomTypeProperty = new CustomType
            {
                Tag = CborTag.Base64Url,
                Value = "SGVsbG8sIFdvcmxkIQ=="
            }
        };

        var cbor = CborSerializer.Serialize(model, options: new() { DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull });

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("CustomTypeProperty");
        reader.ReadTag().Should().Be(CborTag.Base64Url);
        reader.ReadTextString().Should().Be("SGVsbG8sIFdvcmxkIQ==");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void SelfDescribe_Tag_Should_Not_Be_Written_By_Default()
    {
        var model = new TestModel
        {
            IntegerProperty = 42
        };
        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);

        reader.PeekState().Should().NotBe(CborReaderState.Tag);
    }

    [TestMethod]
    public void SelfDescribe_Tag_Should_Be_Written_When_Option_Is_Enabled()
    {
        var model = new TestModel
        {
            IntegerProperty = 42
        };
        var cbor = CborSerializer.Serialize(model, options: new() { WriteSelfDescribeTag = true });

        var reader = new CborReader(cbor);

        reader.PeekState().Should().Be(CborReaderState.Tag);
        reader.PeekTag().Should().Be(CborTag.SelfDescribeCbor);

    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void SelfDescribe_Tag_Should_Be_Always_Accepted(bool writeSelfDescribeTagOption)
    {
        var writer = new CborWriter();
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteStartMap(null);
        writer.WriteTextString("IntegerProperty");
        writer.WriteInt32(123);
        writer.WriteEndMap();

        var cbor = writer.Encode();
        var options = new CborSerializerOptions() { WriteSelfDescribeTag = writeSelfDescribeTagOption };
        var model = CborSerializer.Deserialize<TestModel>(cbor, options);

        model.Should().NotBeNull();
        model.IntegerProperty.Should().Be(123);
    }
}

file class TestModel
{
    public int? IntegerProperty { get; init; }

    [CborConverter<CustomTagAwareConverter>]
    public CustomType? CustomTypeProperty { get; init; }
}

file class CustomType
{
    public CborTag? Tag { get; init; }
    public string? Value { get; init; }
}

file class CustomTagAwareConverter : CborConverter<CustomType>
{
    public override CustomType? Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        return new CustomType
        {
            Tag = tag,
            Value = reader.ReadTextString()
        };
    }

    public override void Write(CborWriter writer, CustomType? value, CborSerializerOptions options)
    {
        if (value is not { Value: { } stringValue, Tag: { } tag })
        {
            throw new CborSerializerException("CustomType value or tag is null");
        }

        writer.WriteTag(tag);
        writer.WriteTextString(stringValue);
    }
}
