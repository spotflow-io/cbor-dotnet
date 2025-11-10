using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class DateTimeOffsetTests
{
    [TestMethod]
    public void Serializing_DateTimeOffset_Should_Not_Write_Tag_By_Default()
    {
        var model = new TestModel
        {
            DateTimeOffsetProperty = new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.FromHours(2))
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("DateTimeOffsetProperty");
        // No tag by default
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15T10:30:45.0000000+02:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_DateTimeOffset_Should_Write_Tag_When_Enabled()
    {
        var model = new TestModel
        {
            DateTimeOffsetProperty = new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.FromHours(2))
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            WriteDateTimeStringTag = true
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("DateTimeOffsetProperty");
        reader.ReadTag().Should().Be(CborTag.DateTimeString);
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15T10:30:45.0000000+02:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_DateTimeString_Tag_And_RFC3339_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTag(CborTag.DateTimeString);
        writer.WriteTextString("2024-01-15T10:30:45+02:00");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeOffsetProperty.Should().Be(new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.FromHours(2)));
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_Without_Tag_As_RFC3339_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTextString("2024-01-15T10:30:45+02:00");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeOffsetProperty.Should().Be(new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.FromHours(2)));
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_UnixTimeSeconds_Tag_And_Integer_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeOffsetProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1705315845));
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_UnixTimeSeconds_Tag_And_Floating_Point_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteDouble(1705315845.5); // 2024-01-15T08:30:45.5Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expected = DateTimeOffset.FromUnixTimeSeconds(1705315845).AddMilliseconds(500);
        model.DateTimeOffsetProperty.Should().BeCloseTo(expected, TimeSpan.FromMilliseconds(1));
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_Without_Tag_As_Unix_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeOffsetProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1705315845));
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_Invalid_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTextString("not-a-date");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-date' could not be parsed as DateTimeOffset.\n\n" +
                "Path:\n" +
                "#0: DateTimeOffsetProperty (*_TestModel)\n\n" +
                "At: byte 35, depth 1.");
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_Invalid_DataType_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteBoolean(true);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'TextString', 'UnsignedInteger' or 'NegativeInteger', got 'Boolean'.\n\n" +
                "Path:\n" +
                "#0: DateTimeOffsetProperty (*_TestModel)\n\n" +
                "At: byte 24, depth 1.");
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTimeOffset_With_UTC_Should_Roundtrip()
    {
        var originalDate = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero);
        var model = new TestModel { DateTimeOffsetProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeOffsetProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTimeOffset_With_Positive_Offset_Should_Roundtrip()
    {
        var originalDate = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.FromHours(5.5));
        var model = new TestModel { DateTimeOffsetProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeOffsetProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTimeOffset_With_Negative_Offset_Should_Roundtrip()
    {
        var originalDate = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.FromHours(-8));
        var model = new TestModel { DateTimeOffsetProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeOffsetProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTimeOffset_Min_Value_Should_Work()
    {
        var model = new TestModel { DateTimeOffsetProperty = DateTimeOffset.MinValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeOffsetProperty.Should().Be(DateTimeOffset.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTimeOffset_Max_Value_Should_Work()
    {
        var model = new TestModel { DateTimeOffsetProperty = DateTimeOffset.MaxValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeOffsetProperty.Should().Be(DateTimeOffset.MaxValue);
    }

    [TestMethod]
    public void Deserializing_DateTimeOffset_With_Negative_Unix_Timestamp_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeOffsetProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(-86400); // 1969-12-31T00:00:00Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeOffsetProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(-86400));
    }

}

file class TestModel
{
    public DateTimeOffset? DateTimeOffsetProperty { get; init; }
}
