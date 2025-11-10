using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class TimeOnlyTests
{
    [TestMethod]
    public void Serializing_TimeOnly_Should_Not_Write_Tag_By_Default()
    {
        var model = new TestModel
        {
            TimeOnlyProperty = new TimeOnly(10, 30, 45)
        };

        var options = new CborSerializerOptions
        {
            PreferNumericPropertyNames = false,
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("TimeOnlyProperty");
        // No tag by default
        var timeString = reader.ReadTextString();
        timeString.Should().Be("10:30:45.0000000");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeOnly_Should_Write_Tag_When_Enabled()
    {
        var model = new TestModel
        {
            TimeOnlyProperty = new TimeOnly(10, 30, 45)
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
        reader.ReadTextString().Should().Be("TimeOnlyProperty");
        // Tag should NOT be written for TimeOnly even when option is enabled
        reader.PeekState().Should().Be(CborReaderState.TextString);
        var timeString = reader.ReadTextString();
        timeString.Should().Be("10:30:45.0000000");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_TimeOnly_With_DateTimeString_Tag_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTag(CborTag.DateTimeString);
        writer.WriteTextString("2024-01-15T10:30:45Z");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        // TimeOnly extracts just the time portion from the full DateTime
        model.TimeOnlyProperty.Should().Be(new TimeOnly(10, 30, 45));
    }

    [TestMethod]
    public void Deserializing_TimeOnly_Without_Tag_As_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTextString("10:30:45");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeOnlyProperty.Should().Be(new TimeOnly(10, 30, 45));
    }

    [TestMethod]
    public void Deserializing_TimeOnly_With_UnixTimeSeconds_Tag_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteInt64(37845); // This represents seconds since epoch, time portion: 10:30:45 UTC
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var expectedTime = TimeOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(37845).DateTime);
        model.TimeOnlyProperty.Should().Be(expectedTime);
    }

    [TestMethod]
    public void Deserializing_TimeOnly_With_UnixTimeSeconds_Tag_And_Floating_Point_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTag(CborTag.UnixTimeSeconds);
        writer.WriteDouble(37845.5); // Fractional seconds
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        var baseTime = TimeOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(37845).DateTime);
        var expectedTime = baseTime.Add(TimeSpan.FromMilliseconds(500));
        model.TimeOnlyProperty.Should().Be(expectedTime);
    }

    [TestMethod]
    public void Deserializing_TimeOnly_With_Invalid_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTextString("not-a-time");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-time' could not be parsed as TimeOnly.*");
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeOnly_Should_Roundtrip()
    {
        var originalTime = new TimeOnly(10, 30, 45, 123);
        var model = new TestModel { TimeOnlyProperty = originalTime };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeOnlyProperty.Should().Be(originalTime);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeOnly_Min_Value_Should_Work()
    {
        var model = new TestModel { TimeOnlyProperty = TimeOnly.MinValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeOnlyProperty.Should().Be(TimeOnly.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeOnly_Max_Value_Should_Work()
    {
        var model = new TestModel { TimeOnlyProperty = TimeOnly.MaxValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeOnlyProperty.Should().Be(TimeOnly.MaxValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeOnly_With_Microseconds_Should_Roundtrip()
    {
        var originalTime = new TimeOnly(10, 30, 45).Add(TimeSpan.FromTicks(1234567));
        var model = new TestModel { TimeOnlyProperty = originalTime };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeOnlyProperty.Should().Be(originalTime);
    }

    [TestMethod]
    public void Deserializing_TimeOnly_Without_Tag_With_Full_DateTime_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeOnlyProperty");
        writer.WriteTextString("2024-01-15T10:30:45Z"); // Full ISO date-time without tag
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string '2024-01-15T10:30:45Z' could not be parsed as TimeOnly.*");
    }
}

file class TestModel
{
    public TimeOnly? TimeOnlyProperty { get; init; }
}
