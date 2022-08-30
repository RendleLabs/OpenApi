using System.Text.Json;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Build;

public class SchemaLoader
{
    private static readonly HashSet<string> IgnoreErrors = new HashSet<string>
    {
        "$schema is not a valid property at #/",
        "$id is not a valid property at #/",
    };
    public OpenApiSchema? LoadSchema(string path, out OpenApiDiagnostic diagnostic)
    {
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