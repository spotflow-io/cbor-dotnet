namespace Spotflow.Cbor;

public abstract class CborNamingPolicy
{
    public static CborNamingPolicy CamelCase { get; } = new CamelCaseCborNamingPolicy();

    public abstract string ConvertName(string name);
}
