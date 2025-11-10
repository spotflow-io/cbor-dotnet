using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class DateOnlyTests
{
    [TestMethod]
    public void Serializing_DateOnly_Should_Not_Write_Tag_By_Default()
    {
        var model = new TestModel
        {
            DateOnlyProperty = new DateOnly(2024, 1, 15)
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("DateOnlyProperty");
        // No tag by default
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_DateOnly_Should_Write_Tag_When_Enabled()
    {
        var model = new TestModel
        {
            DateOnlyProperty = new DateOnly(2024, 1, 15)
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
        reader.ReadTextString().Should().Be("DateOnlyProperty");
        // Tag should NOT be written for DateOnly even when option is enabled
        reader.PeekState().Should().Be(CborReaderState.TextString);
        var dateString = reader.ReadTextString();
        dateString.Should().Be("2024-01-15");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_DateOnly_With_DateTimeString_Tag_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTag(CborTag.DateTimeString);
        writer.WriteTextString("2024-01-15T10:30:45Z");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        // DateOnly extracts just the date portion from the full DateTime
        model.DateOnlyProperty.Should().Be(new DateOnly(2024, 1, 15));
    }

    [TestMethod]
    public void Deserializing_DateOnly_Without_Tag_As_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTextString("2024-01-15");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.DateOnlyProperty.Should().Be(new DateOnly(2024, 1, 15));
    }

    [TestMethod]
    public void Deserializing_DateOnly_Without_Tag_With_Full_DateTime_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTextString("2024-01-15T10:30:45Z"); // Full ISO date-time without tag
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string '2024-01-15T10:30:45Z' could not be parsed as DateOnly.*");
    }

    [TestMethod]
    public void Deserializing_DateOnly_With_UnixTimeSeconds_Tag_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expectedDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime);
        model.DateOnlyProperty.Should().Be(expectedDate);
    }

    [TestMethod]
    public void Deserializing_DateOnly_With_UnixTimeSeconds_Tag_And_Floating_Point_Should_Ignore_Fractional_Part()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteDouble(1705315845.999); // Fractional part should be ignored for DateOnly
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        // DateOnly should only consider the date part, not the time
        var expectedDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime);
        model.DateOnlyProperty.Should().Be(expectedDate);
    }

    [TestMethod]
    public void Deserializing_DateOnly_Without_Tag_As_Unix_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteInt64(1705315845); // 2024-01-15T08:30:45Z
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expectedDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(1705315845).DateTime);
        model.DateOnlyProperty.Should().Be(expectedDate);
    }

    [TestMethod]
    public void Deserializing_DateOnly_With_Invalid_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTextString("not-a-date");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-date' could not be parsed as DateOnly.*");
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateOnly_Should_Roundtrip()
    {
        var originalDate = new DateOnly(2024, 1, 15);
        var model = new TestModel { DateOnlyProperty = originalDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateOnlyProperty.Should().Be(originalDate);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateOnly_Min_Value_Should_Work()
    {
        var model = new TestModel { DateOnlyProperty = DateOnly.MinValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateOnlyProperty.Should().Be(DateOnly.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_DateOnly_Max_Value_Should_Work()
    {
        var model = new TestModel { DateOnlyProperty = DateOnly.MaxValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateOnlyProperty.Should().Be(DateOnly.MaxValue);
    }

    [TestMethod]
    public void Deserializing_DateOnly_With_Negative_Unix_Timestamp_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("DateOnlyProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(-86400); // 1969-12-31
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expectedDate = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(-86400).DateTime);
        model.DateOnlyProperty.Should().Be(expectedDate);
    }

    [TestMethod]
    public void Serializing_DateOnly_Leap_Year_Should_Work()
    {
        var leapYearDate = new DateOnly(2024, 2, 29);
        var model = new TestModel { DateOnlyProperty = leapYearDate };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.DateOnlyProperty.Should().Be(leapYearDate);
    }
}

file class TestModel
{
    public DateOnly? DateOnlyProperty { get; init; }
}
