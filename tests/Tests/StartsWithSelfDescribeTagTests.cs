using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests;

[TestClass]
public class StartsWithSelfDescribeTagTests
{
    [TestMethod]
    public void StartsWithSelfDescribeTag_Should_Return_False_For_Empty_Data()
    {
        var cbor = ReadOnlyMemory<byte>.Empty;

        var result = CborSerializer.StartsWithSelfDescribeTag(cbor);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void StartsWithSelfDescribeTag_Should_Return_True_When_Tag_Is_Present()
    {
        var writer = new CborWriter();
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteInt32(42);
        var cbor = writer.Encode();

        var result = CborSerializer.StartsWithSelfDescribeTag(cbor);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void StartsWithSelfDescribeTag_Should_Return_False_When_Tag_Is_Not_Present()
    {
        var writer = new CborWriter();
        writer.WriteInt32(42);
        var cbor = writer.Encode();

        var result = CborSerializer.StartsWithSelfDescribeTag(cbor);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void StartsWithSelfDescribeTag_Should_Work_Subsequent_Tag()
    {
        var writer = new CborWriter();
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteTag(CborTag.DateTimeString);
        writer.WriteTextString("2024-01-01T00:00:00Z");
        var cbor = writer.Encode();

        var result = CborSerializer.StartsWithSelfDescribeTag(cbor);

        result.Should().BeTrue();
    }

}
