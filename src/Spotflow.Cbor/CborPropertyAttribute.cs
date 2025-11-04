namespace Spotflow.Cbor;

[AttributeUsage(AttributeTargets.Property)]
public class CborPropertyAttribute : Attribute
{
    public string? TextName { get; init; }
    public int NumericName { get; init; } = -1;
}
