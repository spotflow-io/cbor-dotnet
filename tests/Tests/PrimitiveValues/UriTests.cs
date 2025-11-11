using System.Formats.Cbor;

using Spotflow.Cbor;

namespace Tests.PrimitiveValues;

[TestClass]
public class UriTests
{
    [TestMethod]
    public void Serializing_Absolute_Uri_Should_Write_As_TextString()
    {
        var uri = new Uri("https://example.com/path?query=value");
        var model = new TestModel
        {
            UriProperty = uri
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UriProperty");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        reader.ReadTextString().Should().Be("https://example.com/path?query=value");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Serializing_Relative_Uri_Should_Write_As_TextString()
    {
        var uri = new Uri("/path/to/resource", UriKind.Relative);
        var model = new TestModel
        {
            UriProperty = uri
        };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UriProperty");
        reader.PeekState().Should().Be(CborReaderState.TextString);
        reader.ReadTextString().Should().Be("/path/to/resource");
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Absolute_Uri_From_TextString_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com/path");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://example.com/path");
        model.UriProperty.IsAbsoluteUri.Should().BeTrue();
    }

    [TestMethod]
    public void Deserializing_Relative_Uri_From_TextString_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("/relative/path");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("/relative/path");
        model.UriProperty.IsAbsoluteUri.Should().BeFalse();
    }

    [TestMethod]
    public void Deserializing_Uri_With_Query_Parameters_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com/search?q=test&page=1");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://example.com/search?q=test&page=1");
        model.UriProperty.Query.Should().Be("?q=test&page=1");
    }

    [TestMethod]
    public void Deserializing_Uri_With_Fragment_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com/page#section");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://example.com/page#section");
        model.UriProperty.Fragment.Should().Be("#section");
    }

    [TestMethod]
    public void Deserializing_Uri_With_Port_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com:8080/path");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://example.com:8080/path");
        model.UriProperty.Port.Should().Be(8080);
    }

    [TestMethod]
    public void Deserializing_Uri_With_Credentials_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://user:pass@example.com/path");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://user:pass@example.com/path");
        model.UriProperty.UserInfo.Should().Be("user:pass");
    }

    [TestMethod]
    public void Deserializing_Invalid_Uri_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("http://"); // Invalid - scheme without authority
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("The text string 'http://' could not be parsed as URI.*");
    }

    [TestMethod]
    public void Deserializing_Uri_From_Integer_Should_Throw()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteInt64(12345);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var act = () => CborSerializer.Deserialize<TestModel>(cbor);

        act.Should()
            .Throw<CborSerializerException>()
            .WithMessage("Unexpected CBOR data type.*");
    }

    [TestMethod]
    public void Serializing_Null_Nullable_Uri_Should_Write_Null()
    {
        var model = new TestModel { UriProperty = null };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString().Should().Be("UriProperty");
        reader.PeekState().Should().Be(CborReaderState.Null);
        reader.ReadNull();
        reader.ReadEndMap();
    }

    [TestMethod]
    public void Deserializing_Null_To_Nullable_Uri_Should_Work()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteNull();
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().BeNull();
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Absolute_Uri_Should_Roundtrip()
    {
        var uri = new Uri("https://example.com/path?query=value#fragment");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.ToString().Should().Be(uri.ToString());
    }

    [TestMethod]
    public void Serializing_And_Deserializing_Relative_Uri_Should_Roundtrip()
    {
        var uri = new Uri("/relative/path", UriKind.Relative);
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.ToString().Should().Be(uri.ToString());
    }

    [TestMethod]
    [DataRow("https://example.com")]
    [DataRow("http://example.com")]
    [DataRow("ftp://ftp.example.com")]
    [DataRow("file:///C:/path/to/file.txt")]
    [DataRow("mailto:user@example.com")]
    [DataRow("tel:+1234567890")]
    [DataRow("urn:isbn:0451450523")]
    [DataRow("javascript:alert('test')")]
    [DataRow("data:text/plain;base64,SGVsbG8sIFdvcmxkIQ==")]
    [DataRow("http://[::1]:8080/path")]
    [DataRow("http://192.168.1.1:8080/path")]
    [DataRow("http://localhost:3000/api")]
    [DataRow("https://münchen.de/path")]
    public void Serializing_And_Deserializing_Various_Schemes_Should_Roundtrip(string uriString)
    {
        var uri = new Uri(uriString);

        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.ToString().Should().Be(uri.ToString());
    }

    [TestMethod]
    [DataRow("https://example.com/")]
    [DataRow("http://example.com/")]
    [DataRow("ftp://ftp.example.com/")]
    [DataRow("file:///C:/path/to/file.txt")]
    [DataRow("mailto:user@example.com")]
    [DataRow("tel:+1234567890")]
    [DataRow("urn:isbn:0451450523")]
    [DataRow("javascript:alert('test')")]
    [DataRow("data:text/plain;base64,SGVsbG8sIFdvcmxkIQ==")]
    [DataRow("http://[::1]:8080/path")]
    [DataRow("http://192.168.1.1:8080/path")]
    [DataRow("http://localhost:3000/api")]
    [DataRow("https://münchen.de/path")]
    public void Deserializing_Various_Schemes_Should_Parse(string uri)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString(uri);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be(uri);
    }



    [TestMethod]
    public void Serializing_Uri_With_Trailing_Slash_Should_Preserve()
    {
        var uri = new Uri("https://example.com/path/");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.ToString().Should().Be("https://example.com/path/");
    }

    [TestMethod]
    public void Serializing_Uri_Without_Trailing_Slash_Should_Preserve()
    {
        var uri = new Uri("https://example.com/path");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.ToString().Should().Be("https://example.com/path");
    }

    [TestMethod]
    public void Deserializing_Empty_String_As_Uri_Should_Work_As_Relative()
    {
        // Empty string is a valid relative URI in .NET
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString(string.Empty);
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be(string.Empty);
    }

    [TestMethod]
    public void Serializing_Uri_With_Multiple_Query_Parameters_Should_Preserve_Order()
    {
        var uri = new Uri("https://example.com/search?first=1&second=2&third=3");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);
        var deserializedModel = CborSerializer.Deserialize<TestModel>(cbor);

        deserializedModel.Should().NotBeNull();
        deserializedModel.UriProperty.Should().NotBeNull();
        deserializedModel.UriProperty.Query.Should().Be("?first=1&second=2&third=3");
    }

    [TestMethod]
    public void Deserializing_Uri_With_Encoded_Spaces_Should_Parse_Correctly()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com/path%20with%20spaces");
        writer.WriteEndMap();
        var cbor = writer.Encode();
        var model = CborSerializer.Deserialize<TestModel>(cbor);
        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.ToString().Should().Be("https://example.com/path with spaces");
    }

    [TestMethod]
    public void Serializing_Uri_With_Default_Port_Should_Not_Include_Port()
    {
        var uri = new Uri("https://example.com:443/path");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var uriString = reader.ReadTextString();
        reader.ReadEndMap();

        // .NET Uri normalizes default ports
        uriString.Should().Be("https://example.com/path");
    }

    [TestMethod]
    public void Serializing_Uri_With_Non_Default_Port_Should_Include_Port()
    {
        var uri = new Uri("https://example.com:8443/path");
        var model = new TestModel { UriProperty = uri };

        var cbor = CborSerializer.Serialize(model);

        var reader = new CborReader(cbor);
        reader.ReadStartMap();
        reader.ReadTextString();
        var uriString = reader.ReadTextString();
        reader.ReadEndMap();

        uriString.Should().Be("https://example.com:8443/path");
    }

    [TestMethod]
    public void Deserializing_Uri_With_Complex_Path_Should_Parse()
    {
        var writer = new CborWriter();
        writer.WriteStartMap(null);
        writer.WriteTextString("UriProperty");
        writer.WriteTextString("https://example.com/path/to/deep/resource.html");
        writer.WriteEndMap();

        var cbor = writer.Encode();

        var model = CborSerializer.Deserialize<TestModel>(cbor);

        model.Should().NotBeNull();
        model.UriProperty.Should().NotBeNull();
        model.UriProperty.AbsolutePath.Should().Be("/path/to/deep/resource.html");
    }

}

file class TestModel
{
    public Uri? UriProperty { get; init; }
}

