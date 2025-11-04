namespace Spotflow.Cbor;

public class CborDataSerializationException(string message, Exception? innerException) : CborSerializerException(message, innerException)
{
    public CborDataSerializationException(string message) : this(message, null)
    {
    }
}
