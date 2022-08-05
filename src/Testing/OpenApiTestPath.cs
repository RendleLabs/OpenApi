using System.Collections.ObjectModel;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestPath
{
    public OpenApiTestPath(string path, Dictionary<OperationType, OpenApiTest[]> operations)
    {
        Path = path;
        Operations = new ReadOnlyDictionary<OperationType, OpenApiTest[]>(operations);
    }

    public string Path { get; }

    public IReadOnlyDictionary<OperationType, OpenApiTest[]> Operations { get; }
}