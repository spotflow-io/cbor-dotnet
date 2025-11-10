namespace Spotflow.Cbor;

[AttributeUsage(AttributeTargets.Field)]
public class CborStringEnumMemberNameAttribute(string name) : Attribute
{
    public string Name => name;
}
