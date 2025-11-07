namespace Spotflow.Cbor;

[Flags]
public enum CborNumberHandling
{

    /// <summary>
    /// Numbers will only be read from number tokens and will only be written as CBOR numbers.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Numbers can be read from string tokens. Does not prevent numbers from being read from number token.
    /// </summary>
    AllowReadingFromString = 1,

    /// <summary>
    /// Numbers will be written as CBOR strings, not as CBOR numbers.
    /// </summary>
    WriteAsString = 2,
}
