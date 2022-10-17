using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Bundle;

public class SchemaLoader
{
    private static readonly HashSet<string> IgnoreErrors = new()
    {
        "$schema is not a valid property at #/",
        "$id is not a valid property at #/",
    };
    
    public OpenApiSchema? LoadSchema(string path, out OpenApiDiagnostic diagnostic)
    {
        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            
        }
        var text = File.ReadAllText(path);
        var reader = new OpenApiStringReader();
        var schema = reader.ReadFragment<OpenApiSchema>(text, OpenApiSpecVersion.OpenApi3_0, out diagnostic);
        var ignored = diagnostic.Errors.Where(e => IgnoreErrors.Contains(e.Message)).ToArray();
        if (ignored.Length > 0)
        {
            foreach (var error in ignored)
            {
                diagnostic.Errors.Remove(error);
            }
        }
        diagnostic.Write();
        return schema;
    }
}