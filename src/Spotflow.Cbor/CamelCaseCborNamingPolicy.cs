namespace Spotflow.Cbor;

internal sealed class CamelCaseCborNamingPolicy : CborNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
        {
            return name;
        }

        return string.Create(name.Length, name, (chars, name) =>
        {
            name.CopyTo(chars);
            UpdateCasing(chars);
        });
    }

    private static void UpdateCasing(Span<char> chars)
    {
        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            var hasNext = (i + 1 < chars.Length);

            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }
    }
}
