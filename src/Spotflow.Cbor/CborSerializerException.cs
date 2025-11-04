namespace Spotflow.Cbor;

public class CborSerializerException(string message, Exception? innerException) : Exception(message, innerException)
{
    public CborSerializerException(string message) : this(message, null)
    {
    }
}
