using System.Collections.ObjectModel;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestDocument
{
    public OpenApiTestDocument(string? server, Dictionary<string, OpenApiTestPath> tests)
    {
        Server = server;
        Tests = new ReadOnlyDictionary<string, OpenApiTestPath>(tests);
    }

    public string? Server { get; }
    
    public IReadOnlyDictionary<string, OpenApiTestPath> Tests { get; }
}