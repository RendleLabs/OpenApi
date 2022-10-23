namespace RendleLabs.OpenApi.Generator.Controllers;

internal record ActionMethodParameter(string? From, string Type, string Name)
{
    public override string ToString()
    {
        return From is not null ? $"[From{From}] {Type} {Name}" : $"{Type} {Name}";
    }
}