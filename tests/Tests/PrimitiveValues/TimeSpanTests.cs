using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class TimeSpanTests
{
    [TestMethod]
    public void Serializing_TimeSpan_Should_Write_As_DotNet_Format()
    {
        var model = new TestModel
        {
            TimeSpanProperty = new TimeSpan(1, 2, 30, 45) // 1 day, 2 hours, 30 minutes, 45 seconds
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("TimeSpanProperty");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        var durationString = reader.ReadTextString();
        durationString.Should().Be("1.02:30:45");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Milliseconds_Should_Include_Fractional_Seconds()
    {
        var model = new TestModel
        {
            TimeSpanProperty = new TimeSpan(0, 1, 30, 45, 500) // 1 hour, 30 minutes, 45.5 seconds
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("01:30:45.5000000");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeSpan_Zero_Should_Write_00_00_00()
    {
        var model = new TestModel
        {
            TimeSpanProperty = TimeSpan.Zero
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("00:00:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Negative_TimeSpan_Should_Include_Minus_Sign()
    {
        var model = new TestModel
        {
            TimeSpanProperty = new TimeSpan(-1, -2, -30, -45) // -1 day, -2 hours, -30 minutes, -45 seconds
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("-1.02:30:45");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_DotNet_Format_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("1.02:30:45");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(new TimeSpan(1, 2, 30, 45));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_DotNet_Format_With_Fractional_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("01:30:45.5000000");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(new TimeSpan(0, 1, 30, 45, 500));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Short_Format_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("1:30:45"); // Short format (no days)
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(new TimeSpan(1, 30, 45));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Integer_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteInt64(3661); // 1 hour, 1 minute, 1 second
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(TimeSpan.FromSeconds(3661));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Floating_Point_Seconds_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteDouble(90.5); // 1 minute, 30.5 seconds
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().BeCloseTo(TimeSpan.FromSeconds(90.5), TimeSpan.FromMilliseconds(1));
    }

    [TestMethod]
    public void Deserializing_Negative_TimeSpan_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("-01:30:00");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(new TimeSpan(-1, -30, 0));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Invalid_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("not-a-timespan");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-timespan' could not be parsed as TimeSpan.*");
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Boolean_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteBoolean(true);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type.*");
    }

    [TestMethod]
    public void Serializing_Null_Nullable_TimeSpan_Should_Write_Null()
    {
        var model = new TestModel { TimeSpanProperty = null };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("TimeSpanProperty");
        reader.PeekState().Should().Be(CborReaderState.Null);
        reader.ReadNull();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Null_To_Nullable_TimeSpan_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().BeNull();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeSpan_Should_Roundtrip()
    {
        var originalTimeSpan = new TimeSpan(2, 3, 45, 30, 123);
        var model = new TestModel { TimeSpanProperty = originalTimeSpan };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Value.Should().Be(originalTimeSpan);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeSpan_MinValue_Should_Work()
    {
        var model = new TestModel { TimeSpanProperty = TimeSpan.MinValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Value.Should().Be(TimeSpan.MinValue);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_TimeSpan_MaxValue_Should_Work()
    {
        var model = new TestModel { TimeSpanProperty = TimeSpan.MaxValue };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Value.Should().Be(TimeSpan.MaxValue);
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Only_Hours_Should_Format_Correctly()
    {
        var model = new TestModel
        {
            TimeSpanProperty = TimeSpan.FromHours(5)
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("05:00:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Only_Minutes_Should_Format_Correctly()
    {
        var model = new TestModel
        {
            TimeSpanProperty = TimeSpan.FromMinutes(45)
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("00:45:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Only_Seconds_Should_Format_Correctly()
    {
        var model = new TestModel
        {
            TimeSpanProperty = TimeSpan.FromSeconds(30)
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("00:00:30");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Only_Days_Should_Format_Correctly()
    {
        var model = new TestModel
        {
            TimeSpanProperty = TimeSpan.FromDays(3)
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var durationString = reader.ReadTextString();
        durationString.Should().Be("3.00:00:00");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Zero_String_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("00:00:00");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Negative_Seconds_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteInt64(-3600); // -1 hour
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        model.TimeSpanProperty.Value.Should().Be(TimeSpan.FromHours(-1));
    }

    [TestMethod]
    public void Serializing_TimeSpan_With_Ticks_Should_Include_Fractional_Seconds()
    {
        var model = new TestModel
        {
            TimeSpanProperty = new TimeSpan(0, 0, 0, 1, 0).Add(TimeSpan.FromTicks(1234567)) // 1 second + ticks
        };

        var options = new CborSerializerOptions
        {
            DefaultIgnoreCondition = CborIgnoreCondition.WhenWritingNull
        };

        var cbor = CborSerializer.Serialize(model, options);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor, options);

        deserializedModel.Should().NotBeNull();
        deserializedModel.TimeSpanProperty.Should().NotBeNull();
        // Roundtrip should preserve the exact value
        deserializedModel.TimeSpanProperty.Value.Should().Be(model.TimeSpanProperty!.Value);
    }

    [TestMethod]
    public void Serializing_Multiple_TimeSpans_In_Object_Should_Work()
    {
        var model = new MultipleTimeSpanModel
        {
            Duration1 = TimeSpan.FromHours(2),
            Duration2 = TimeSpan.FromMinutes(30),
            Duration3 = TimeSpan.FromDays(1)
        };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<MultipleTimeSpanModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.Duration1.Should().NotBeNull();
        deserializedModel.Duration1.Value.Should().Be(TimeSpan.FromHours(2));
        deserializedModel.Duration2.Should().NotBeNull();
        deserializedModel.Duration2.Value.Should().Be(TimeSpan.FromMinutes(30));
        deserializedModel.Duration3.Should().NotBeNull();
        deserializedModel.Duration3.Value.Should().Be(TimeSpan.FromDays(1));
    }

    [TestMethod]
    public void Deserializing_TimeSpan_From_Complex_Format_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("TimeSpanProperty");
        writer.WriteTextString("2.03:04:05.1230000"); // 2 days, 3 hours, 4 minutes, 5.123 seconds
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.TimeSpanProperty.Should().NotBeNull();
        var expected = new TimeSpan(2, 3, 4, 5, 123);
        model.TimeSpanProperty.Value.Should().Be(expected);
    }
}

file class TestModel
{
    public TimeSpan? TimeSpanProperty { get; init; }
}

file class MultipleTimeSpanModel
{
    public TimeSpan? Duration1 { get; init; }
    public TimeSpan? Duration2 { get; init; }
    public TimeSpan? Duration3 { get; init; }
}
