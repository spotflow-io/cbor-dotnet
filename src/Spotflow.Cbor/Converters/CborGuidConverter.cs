using System.Formats.Cbor;

namespace Spotflow.Cbor.Converters;

internal class CborGuidConverter : CborConverter<Guid>
{
    // https://www.iana.org/assignments/cbor-tags/cbor-tags.xhtml
    private const CborTag _uuidTag = (CborTag) 37;
    private const int _guidByteLength = 16;
    private const int _guidMaxTextLength = 38;
    private static readonly string[] _allowedReadFormats = ["D", "N", "B", "P"];


    public override bool HandleNull => false;

    public override Guid Read(CborReader reader, Type typeToConvert, CborTag? tag, CborSerializerOptions options)
    {
        var state = reader.PeekState();

        // Handle GUID from byte string

        if (state is CborReaderState.ByteString)
        {
            Span<byte> bytes = stackalloc byte[_guidByteLength];

            if (!reader.TryReadByteString(bytes, out var bytesWritten) || bytesWritten is not _guidByteLength)
            {
                throw new CborSerializerException($"Expected 16 bytes for GUID, but got {bytes.Length} bytes.");
            }

            return new Guid(bytes, bigEndian: true);
        }

        if (tag is _uuidTag)
        {
            throw new CborSerializerException($"Expected byte string for date item with tag 37 (UUID), but got different data type ({state}).");
        }

        // Handle GUID from text string

        if (state is CborReaderState.TextString)
        {
            Span<char> chars = stackalloc char[_guidMaxTextLength];

            if (!reader.TryReadTextString(chars, out var charsWritten) || charsWritten is > _guidMaxTextLength)
            {
                throw new CborSerializerException($"Expected text string up to {_guidMaxTextLength} characters for GUID, but got {charsWritten} characters.");
            }

            chars = chars[..charsWritten];

            foreach (var format in _allowedReadFormats)
            {
                if (Guid.TryParseExact(chars, format, out var guid))
                {
                    return guid;
                }
            }

            throw new CborSerializerException($"The text string '{new string(chars)}' could not be parsed as Guid.");

        }

        throw UnexpectedDataType([CborReaderState.ByteString, CborReaderState.TextString], state);
    }

    public override void Write(CborWriter writer, Guid value, CborSerializerOptions options)
    {
        // Write GUID as byte string (16 bytes) by default

        Span<byte> bytes = stackalloc byte[16];

        if (!value.TryWriteBytes(bytes, bigEndian: true, out _))
        {
            throw new InvalidOperationException($"Failed to write Guid value {value} as bytes.");
        }

        writer.WriteByteString(bytes);
    }
}
