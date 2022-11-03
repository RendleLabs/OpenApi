using System.Collections.ObjectModel;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestPath
{
    public OpenApiTestPath(string path, Dictionary<OperationType, OpenApiTestElement[]> operations)
    {
        Path = path;
        Operations = new ReadOnlyDictionary<OperationType, OpenApiTestElement[]>(operations);
    }

    public string Path { get; }

    public IReadOnlyDictionary<OperationType, OpenApiTestElement[]> Operations { get; }
}