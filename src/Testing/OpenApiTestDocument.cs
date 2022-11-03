using System.Collections.ObjectModel;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestDocument
{
    public OpenApiTestDocument(string? server, List<OpenApiTestElement> tests)
    {
        Server = server;
        Tests = tests.ToArray();
    }

    public string? Server { get; }
    
    public OpenApiTestElement[] Tests { get; }
}