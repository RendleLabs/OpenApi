using System.CodeDom.Compiler;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public class ApiFirstGenerator
{
    
}

public class ApiBaseGenerator
{
    private readonly string _projectNamespace;
    private readonly OpenApiDocument _document;
    private readonly string _tag;

    private static readonly string[] Usings =
    {
        "System.Diagnostics.CodeAnalysis",
        "ApiBase.Models",
        "Microsoft.AspNetCore.Mvc",
    };
    
    private readonly IndentedTextWriter _writer;

    public ApiBaseGenerator(TextWriter writer, string projectNamespace, OpenApiDocument document, string tag)
    {
        _projectNamespace = projectNamespace;
        _document = document;
        _tag = tag;
        _writer = new IndentedTextWriter(writer, "    ");
    }

    public async Task Write()
    {
        await WriteUsings();
        await _writer.WriteLineAsync();
        await _writer.WriteLineAsync($"namespace {_projectNamespace}.Api;");
        await _writer.WriteLineAsync();
        await _writer.WriteLineAsync("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]");
        await _writer.WriteLineAsync($"public abstract class {_tag}Base");
        using (_writer.OpenBrace())
        {
            await _writer.WriteLineAsync($"private static void __Map<T>(WebApplication app, Func<IServiceProvider, T> builder) where T : {_tag}Base");
            using (_writer.OpenBrace())
            {
                foreach (var (path, pathItem) in _document.Paths)
                {
                    await WritePathMaps(pathItem, path);
                }
            }
        }
    }

    private async Task WritePathMaps(OpenApiPathItem pathItem, string path)
    {
        var operations = pathItem.Operations
            .Where(o => o.Value.Tags.Any(t => t.Name.Equals(_tag)));
        foreach (var (operationType, operation) in operations)
        {
            if (operation.OperationId is not { Length: > 0 } operationId)
            {
                operationId = operationType.ToString();
            }
            await _writer.WriteAsync($"app.Map{operationType}(\"{path}\", (");
            
            foreach (var parameter in pathItem.GetPathParameters())
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                await _writer.WriteAsync($"{type} {parameter.Name}, ");
            }

            await _writer.WriteLineAsync("HttpContext context) =>");
            await _writer.WriteLineAsync('{');
            
            using (_writer.OpenIndent())
            {
                await _writer.WriteLineAsync("var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);");
                await _writer.WriteAsync($"return impl.{operationId}(");
                foreach (var parameter in pathItem.Parameters.Where(p => p.In == ParameterLocation.Path))
                {
                    await _writer.WriteAsync($"{parameter.Name}, ");
                }

                await _writer.WriteLineAsync("context);");
            }

            await _writer.WriteLineAsync("});");
        }
    }

    private async Task WriteUsings()
    {
        foreach (var u in Usings)
        {
            await _writer.WriteLineAsync($"using {u};");
        }
    }
}

internal static class SchemaHelpers
{
    public static string SchemaTypeToDotNetType(OpenApiSchema schema)
    {
        var type = schema.Type switch
        {
            "boolean" => "bool",
            "number" => "double",
            "string" => StringSchemaType(schema),
            "integer" => SchemaTypeToInteger(schema),
            _ => "object",
        };
        if (schema.Nullable) type += '?';
        return type;
    }

    private static string StringSchemaType(OpenApiSchema schema)
    {
        return schema.Format switch
        {
            "date-time" => "DateTime",
            "time" => "TimeOnly",
            "date" => "DateOnly",
            "duration" => "TimeSpan",
            "uuid" => "Guid",
            "uri" => "Uri",
            _ => "string",
        };
    }

    private static string SchemaTypeToInteger(OpenApiSchema schema)
    {
        return schema.Maximum > int.MaxValue ? "long" : "int";
    }
}

internal static class PathItemHelpers
{
    public static IEnumerable<OpenApiParameter> GetPathParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Path);
    
    public static IEnumerable<OpenApiParameter> GetQueryParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Query);
    
    public static IEnumerable<OpenApiParameter> GetHeaderParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Header);
    
    public static IEnumerable<OpenApiParameter> GetCookieParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Cookie);
    
    private static IEnumerable<OpenApiParameter> GetParametersIn(OpenApiPathItem pathItem, ParameterLocation location) =>
        pathItem.Parameters.Where(p => p.In == location);
}