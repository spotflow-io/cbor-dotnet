namespace Spotflow.Cbor;

[Flags]
public enum CborBooleanHandling
{
    /// <summary>
    /// Booleans will only be read from CBOR boolean tokens.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Booleans can be read from integer tokens (0 for false, otherwise true).
    /// </summary>
    AllowReadingFromInteger = 1,

    /// <summary>
    /// Booleans can be read from string tokens ("true", "false"), case-insensitively.
    /// Does not prevent booleans from being read from CBOR boolean tokens.
    /// </summary>
    AllowReadingFromString = 2
}
