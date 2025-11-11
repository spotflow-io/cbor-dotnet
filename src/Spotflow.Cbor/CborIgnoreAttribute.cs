namespace Spotflow.Cbor;

/// <summary>
/// Prevents a property from being serialized or deserialized.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CborIgnoreAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the condition under which the property should be ignored.
    /// </summary>
    public CborIgnoreCondition Condition { get; init; } = CborIgnoreCondition.Always;
}
