using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class GuidTests
{
    [TestMethod]
    public void Serializing_Guid_Should_Write_As_ByteString()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var model = new TestModel
        {
            GuidProperty = guid
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("GuidProperty");
        reader.PeekState().Should().Be(CborReaderState.ByteString);
        var bytes = reader.ReadByteString();
        bytes.Length.Should().Be(16);
        new Guid(bytes, bigEndian: true).Should().Be(guid);
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Guid_From_ByteString_Should_Parse()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");

        Span<byte> guidBytes = stackalloc byte[16];
        guid.TryWriteBytes(guidBytes, bigEndian: true, out _);
        writer.WriteByteString(guidBytes);

        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_TextString_Should_Parse()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_TextString_Without_Dashes_Should_Parse()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString("N")); // Format without dashes
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_TextString_With_Braces_Should_Parse()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString("B")); // Format with braces
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_TextString_With_Parentheses_Should_Parse()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString("P")); // Format with parentheses
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_Invalid_ByteString_Length_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteByteString(new byte[] { 1, 2, 3, 4, 5 }); // Invalid length (not 16)
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("*Expected 16 bytes for GUID*");
    }

    [TestMethod]
    public void Deserializing_Guid_From_Invalid_TextString_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString("not-a-valid-guid");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'not-a-valid-guid' could not be parsed as Guid.*");
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Guid_Should_Roundtrip()
    {
        var guid = Guid.Parse("a1b2c3d4-e5f6-1234-5678-9abcdef01234");
        var model = new TestModel { GuidProperty = guid };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.GuidProperty.Should().NotBeNull();
        deserializedModel.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Guid_Empty_Should_Roundtrip()
    {
        var model = new TestModel { GuidProperty = Guid.Empty };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.GuidProperty.Should().NotBeNull();
        deserializedModel.GuidProperty.Value.Should().Be(Guid.Empty);
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Random_Guids_Should_Roundtrip()
    {
        for (int i = 0; i < 10; i++)
        {
            var guid = Guid.NewGuid();
            var model = new TestModel { GuidProperty = guid };

            var cbor = CborSerializer.Serialize(model);
            var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

            deserializedModel.Should().NotBeNull();
            deserializedModel.GuidProperty.Should().NotBeNull();
            deserializedModel.GuidProperty.Value.Should().Be(guid);
        }
    }

    [TestMethod]
    public void Serializing_Guid_With_All_Zeros_Should_Work()
    {
        var guid = new Guid(new byte[16]); // All zeros
        var model = new TestModel { GuidProperty = guid };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.GuidProperty.Should().NotBeNull();
        deserializedModel.GuidProperty.Value.Should().Be(guid);
        deserializedModel.GuidProperty.Value.Should().Be(Guid.Empty);
    }

    [TestMethod]
    public void Serializing_Guid_With_All_Ones_Should_Work()
    {
        var allOnes = new byte[16];
        Array.Fill(allOnes, (byte) 0xFF);
        var guid = new Guid(allOnes);
        var model = new TestModel { GuidProperty = guid };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.GuidProperty.Should().NotBeNull();
        deserializedModel.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_Integer_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteInt64(12345);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type.*");
    }

    [TestMethod]
    public void Serializing_Null_Nullable_Guid_Should_Write_Null()
    {
        var model = new TestModel { GuidProperty = null };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("GuidProperty");
        reader.PeekState().Should().Be(CborReaderState.Null);
        reader.ReadNull();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Null_To_Nullable_Guid_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().BeNull();
    }

    [TestMethod]
    public void Serializing_Guid_Maintains_Byte_Order()
    {
        // Test that byte order is preserved during serialization/deserialization roundtrip
        // Using RFC 4122 network byte order (big-endian)
        var guid = Guid.NewGuid();
        var model = new TestModel { GuidProperty = guid };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var serializedBytes = reader.ReadByteString();
        reader.ReadEndMap();

        serializedBytes.Length.Should().Be(16);

        // Deserialize the bytes back using bigEndian: true to verify roundtrip
        var reconstructedGuid = new Guid(serializedBytes, bigEndian: true);
        reconstructedGuid.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_Uppercase_String_Should_Parse()
    {
        var guid = Guid.Parse("ABCDEF12-3456-7890-ABCD-EF1234567890");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString().ToUpper());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_Lowercase_String_Should_Parse()
    {
        var guid = Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890");
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guid.ToString().ToLower());
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);
    }

    [TestMethod]
    public void Deserializing_Guid_From_Format_N_Should_Parse()
    {
        // Format N: 32 digits with no hyphens
        // Example: 00000000000000000000000000000000
        var guid = Guid.NewGuid();
        var guidString = guid.ToString("N");

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guidString);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);

        // Verify format N was used
        guidString.Should().HaveLength(32);
        guidString.Should().NotContain("-");
        guidString.Should().NotContain("{");
        guidString.Should().NotContain("(");
    }

    [TestMethod]
    public void Deserializing_Guid_From_Format_D_Should_Parse()
    {
        // Format D: 32 digits separated by hyphens (default format)
        // Example: 00000000-0000-0000-0000-000000000000
        var guid = Guid.NewGuid();
        var guidString = guid.ToString("D");

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guidString);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);

        // Verify format D was used
        guidString.Should().HaveLength(36);
        guidString.Should().Contain("-");
        guidString.Should().NotContain("{");
        guidString.Should().NotContain("(");
    }

    [TestMethod]
    public void Deserializing_Guid_From_Format_B_Should_Parse()
    {
        // Format B: 32 digits separated by hyphens, enclosed in braces
        // Example: {00000000-0000-0000-0000-000000000000}
        var guid = Guid.NewGuid();
        var guidString = guid.ToString("B");

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guidString);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);

        // Verify format B was used
        guidString.Should().HaveLength(38);
        guidString.Should().StartWith("{");
        guidString.Should().EndWith("}");
        guidString.Should().Contain("-");
    }

    [TestMethod]
    public void Deserializing_Guid_From_Format_P_Should_Parse()
    {
        // Format P: 32 digits separated by hyphens, enclosed in parentheses
        // Example: (00000000-0000-0000-0000-000000000000)
        var guid = Guid.NewGuid();
        var guidString = guid.ToString("P");

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guidString);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(guid);

        // Verify format P was used
        guidString.Should().HaveLength(38);
        guidString.Should().StartWith("(");
        guidString.Should().EndWith(")");
        guidString.Should().Contain("-");
    }

    [TestMethod]
    [DataRow("N")]
    [DataRow("D")]
    [DataRow("B")]
    [DataRow("P")]
    public void Deserializing_Guid_All_Standard_Formats_Should_Roundtrip(string format)
    {
        var originalGuid = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        var guidString = originalGuid.ToString(format);

        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(guidString);
        writer.WriteEndMap();

        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.GuidProperty.Should().NotBeNull();
        model.GuidProperty.Value.Should().Be(originalGuid, $"because format {format} should parse correctly");

    }

    [TestMethod]
    [DataRow("N")]
    [DataRow("D")]
    [DataRow("B")]
    [DataRow("P")]
    public void Deserializing_Guid_Mixed_Case_All_Formats_Should_Parse(string format)
    {
        var originalGuid = Guid.Parse("FEDCBA98-7654-3210-FEDC-BA9876543210");
        var formats = new[] { "N", "D", "B", "P" };


        // Test lowercase
        var guidStringLower = originalGuid.ToString(format).ToLowerInvariant();
        var writerLower = new CborWriter();
        writerLower.WriteStartMap(null);
        writerLower.WriteTextString("GuidProperty");
        writerLower.WriteTextString(guidStringLower);
        writerLower.WriteEndMap();

        var cborLower = writerLower.Encode();
        var modelLower = CborSerializer.Deserialize<TestModel>(cborLower);
        modelLower.Should().NotBeNull();
        modelLower.GuidProperty.Should().NotBeNull();
        modelLower.GuidProperty.Value.Should().Be(originalGuid, $"because format {format} (lowercase) should parse correctly");

        // Test uppercase
        var guidStringUpper = originalGuid.ToString(format).ToUpperInvariant();
        var writerUpper = new CborWriter();
        writerUpper.WriteStartMap(null);
        writerUpper.WriteTextString("GuidProperty");
        writerUpper.WriteTextString(guidStringUpper);
        writerUpper.WriteEndMap();

        var cborUpper = writerUpper.Encode();
        var modelUpper = CborSerializer.Deserialize<TestModel>(cborUpper);
        modelUpper.Should().NotBeNull();
        modelUpper.GuidProperty.Should().NotBeNull();
        modelUpper.GuidProperty.Value.Should().Be(originalGuid, $"because format {format} (uppercase) should parse correctly");

    }

    [TestMethod]
    public void Deserializing_Guid_With_Mixed_Format_Characters_Should_Throw()
    {
        // Invalid GUID: has both braces and parentheses
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString("{12345678-1234-1234-1234-123456789abc)"); // Mismatched brackets
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("*could not be parsed as Guid.*");
    }

    [TestMethod]
    public void Deserializing_Guid_With_Invalid_Hex_Characters_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString("12345678-1234-1234-1234-12345678GHIJ"); // Invalid hex chars G, H, I, J
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("*could not be parsed as Guid.*");
    }

    [TestMethod]
    public void Deserializing_Guid_With_Wrong_Number_Of_Hyphens_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString("12345678-1234-1234-123456789abc"); // Missing one hyphen
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("*could not be parsed as Guid.*");
    }

    [TestMethod]
    public void Deserializing_Guid_Empty_String_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("GuidProperty");
        writer.WriteTextString(string.Empty);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("*could not be parsed as Guid.*");
    }
}

file class TestModel
{
    public Guid? GuidProperty { get; init; }
}
