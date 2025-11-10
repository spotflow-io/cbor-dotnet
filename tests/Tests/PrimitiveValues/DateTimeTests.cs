using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class DateTimeTests
{
    [TestMethod]
    public void Serializing_DateTime_Should_Not_Write_Tag_By_Default()
    {
        var model = new TestModel
        {
            DateTimeProperty = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc)
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("DateTimeProperty");
        // No tag by default
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15T10:30:45.0000000Z");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_DateTime_Should_Write_Tag_When_Enabled()
    {
        var model = new TestModel
        {
            DateTimeProperty = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc)
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull,
            WriteDateTimeStringTag = true
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("DateTimeProperty");
        reader.ReadTag().Should().Be(CborTag.DateTimeString);
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15T10:30:45.0000000Z");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_DateTime_With_DateTimeString_Tag_And_RFC3339_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTag(CborTag.DateTimeString);
        writer.WriteTextString("2024-01-15T10:30:45Z");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeProperty.Should().Be(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
    }

    [TestMethod]
    public void Deserializing_DateTime_Without_Tag_As_RFC3339_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTextString("2024-01-15T10:30:45Z");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeProperty.Should().Be(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
    }

    [TestMethod]
    public void Deserializing_DateTime_With_UnixTimeSeconds_Tag_And_Integer_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime);
    }

    [TestMethod]
    public void Deserializing_DateTime_With_UnixTimeSeconds_Tag_And_Floating_Point_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteDouble(1705315845.5); // 2024-01-15T08:30:45.5Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expected = DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime.AddMilliseconds(500);
        model.DateTimeProperty.Should().BeCloseTo(expected, TimeSpan.FromMilliseconds(1));
    }

    [TestMethod]
    public void Deserializing_DateTime_Without_Tag_As_Unix_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime);
    }

    [TestMethod]
    public void Deserializing_DateTime_With_Invalid_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTextString("not-a-date");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-date' could not be parsed as DateTime.\n\n" +
                "Path:\n" +
                "#0: DateTimeProperty (*_TestModel)\n\n" +
                "At: byte 29, depth 1.");
    }

    [TestMethod]
    public void Deserializing_DateTime_With_Invalid_DataType_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteBoolean(true);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type. Expected 'TextString', 'UnsignedInteger' or 'NegativeInteger', got 'Boolean'.\n\n" +
                "Path:\n" +
                "#0: DateTimeProperty (*_TestModel)\n\n" +
                "At: byte 18, depth 1.");
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTime_With_UTC_Should_Roundtrip()
    {
        var originalDate = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc);
        var model = new TestModel { DateTimeProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTime_With_Local_Should_Roundtrip()
    {
        var originalDate = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Local);
        var model = new TestModel { DateTimeProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTime_With_Unspecified_Should_Roundtrip()
    {
        var originalDate = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Unspecified);
        var model = new TestModel { DateTimeProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTime_Min_Value_Should_Work()
    {
        var model = new TestModel { DateTimeProperty = DateTime.MinValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeProperty.Should().Be(DateTime.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateTime_Max_Value_Should_Work()
    {
        var model = new TestModel { DateTimeProperty = DateTime.MaxValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateTimeProperty.Should().Be(DateTime.MaxValue);
    }

    [TestMethod]
    public void Deserializing_DateTime_With_Negative_Unix_Timestamp_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateTimeProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(-86400); // 1969-12-31T00:00:00Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateTimeProperty.Should().Be(DateTimeOffset.FromUnixTimeSeconds(-86400).DateTime);
    }
}

file class TestModel
{
    public DateTime? DateTimeProperty { get; init; }
}

