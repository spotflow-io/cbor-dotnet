using System.Formats.Cbor;
using System.Text;

namespace Spotflow.Cbor;

public class CborSerializerException : Exception
{
    private readonly string _scopelessMessage;

    public CborSerializerException(string scopelessMessage, Exception? innerException = null) : base(message: null, innerException)
    {
        Scopes = null;
        _scopelessMessage = scopelessMessage;
    }

    private CborSerializerException(CborSerializerException childException, string? parentScope, (int Byte, int Depth)? positionInfo) : base(message: null, childException)
    {
        if (parentScope is null)
        {
            Scopes = childException.Scopes;
        }
        else
        {
            Scopes = [.. (childException.Scopes ?? []), parentScope];
        }

        PositionInfo = positionInfo;

        _scopelessMessage = childException._scopelessMessage;
    }

    public override string Message
    {
        get
        {
            var builder = new StringBuilder();

            builder.Append(_scopelessMessage);

            if (Scopes?.Length > 0)
            {
                AppendScopes(Scopes, builder);
            }

            if (PositionInfo is not null)
            {
                AppendPositionInfo(PositionInfo.Value, builder);
            }

            return builder.ToString();

        }
    }

    /// <summary>
    /// Hierarchy of scopes leading to the error. Top-level scope is last.
    /// </summary>
    internal string[]? Scopes { get; }

    internal (int, int)? PositionInfo { get; }

    internal static bool IsRecognizedException(Exception ex) => ex is CborSerializerException or NotSupportedException or FormatException or CborContentException;

    internal static Exception WrapWithParentScope(Exception ex, string parentScope)
    {
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentScope);

        return ex switch
        {
            CborSerializerException cborSerializerException => new CborSerializerException(cborSerializerException, parentScope, null),
            NotSupportedException nse => WrapWithAdditionalContext(nse, parentScope, null, (m, ie) => new NotSupportedException(m, ie)),
            FormatException fe => WrapWithAdditionalContext(fe, parentScope, null, (m, ie) => new FormatException(m, ie)),
            CborContentException cce => WrapWithAdditionalContext(cce, parentScope, null, (m, ie) => new CborSerializerException(m, ie)),
            _ => new InvalidOperationException("Unrecognized exception type cannot be wrapped with scope information.", ex)
        };
    }

    internal static Exception WrapWithPositionInfo(Exception ex, int currentByte, int currentDepth)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var positionInfo = (currentByte, currentDepth);

        return ex switch
        {
            CborSerializerException cborSerializerException => new CborSerializerException(cborSerializerException, null, (currentByte, currentDepth)),
            NotSupportedException nse => WrapWithAdditionalContext(nse, null, positionInfo, (m, ie) => new NotSupportedException(m, ie)),
            FormatException fe => WrapWithAdditionalContext(fe, null, positionInfo, (m, ie) => new FormatException(m, ie)),
            CborContentException cce => WrapWithAdditionalContext(cce, null, positionInfo, (m, ie) => new CborSerializerException(m, ie)),
            _ => new InvalidOperationException("Unrecognized exception type cannot be wrapped with position information.", ex)
        };
    }

    private static T WrapWithAdditionalContext<T>(Exception originalException, string? parentScope, (int Byte, int Depth)? positionInfo, Func<string, Exception, T> factory) where T : Exception
    {
        var wrappingMessageBuilder = new StringBuilder(originalException.Message);

        const string wrappingExceptionDataMarker = "ContainsCborSerializerScopes";

        if (parentScope is not null)
        {
            if (originalException.Data[wrappingExceptionDataMarker] is true)
            {
                AppendSingleScope(parentScope, wrappingMessageBuilder);
            }
            else
            {
                AppendScopes([parentScope], wrappingMessageBuilder);
            }
        }

        if (positionInfo is not null)
        {
            AppendPositionInfo(positionInfo.Value, wrappingMessageBuilder);
        }

        var wrappingExceptionMessage = wrappingMessageBuilder.ToString();

        var wrappingException = factory(wrappingExceptionMessage, originalException);

        wrappingException.Data[wrappingExceptionDataMarker] = true;

        return wrappingException;
    }

    private static StringBuilder AppendScopes(string[] scopes, StringBuilder builder)
    {
        builder.Append("\n\nPath:");

        for (var i = 0; i < scopes.Length; i++)
        {
            AppendSingleScope(scopes[i], builder);
        }

        return builder;
    }

    private static StringBuilder AppendSingleScope(string parentScope, StringBuilder builder)
    {
        builder.Append('\n');
        builder.Append(parentScope);

        return builder;
    }

    private static StringBuilder AppendPositionInfo((int Byte, int Depth) positionInfo, StringBuilder builder)
    {
        builder.Append($"\n\nAt: byte {positionInfo.Byte}, depth {positionInfo.Depth}.");
        return builder;
    }
}
