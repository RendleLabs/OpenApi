using System.CodeDom.Compiler;
using System.Net;
using System.Text.RegularExpressions;
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
        foreach (var (operationType, operation) in pathItem.GetOperationsWithTag(_tag))
        {
            if (operation.OperationId is not { Length: > 0 } operationId)
            {
                operationId = operationType.ToString();
            }

            await _writer.WriteAsync($"app.Map{operationType}(\"{path}\", (");

            var cSharpParameters = new List<string>();
            string cSharpName;
            foreach (var parameter in pathItem.GetPathParameters())
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                cSharpParameters.Add(cSharpName = parameter.CSharpName());
                await _writer.WriteAsync($"{type} {cSharpName}, ");
            }

            foreach (var parameter in pathItem.GetQueryParameters())
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                cSharpParameters.Add(cSharpName = parameter.CSharpName());
                await _writer.WriteAsync($"{type} {cSharpName}, ");
            }

            foreach (var parameter in pathItem.GetHeaderParameters())
            {
                cSharpParameters.Add(cSharpName = parameter.CSharpName());
                await _writer.WriteAsync($"[FromHeader(Name = \"{parameter.Name}\"] string {cSharpName}");
            }

            await _writer.WriteLineAsync("HttpContext context) =>");
            await _writer.WriteLineAsync('{');

            using (_writer.OpenIndent())
            {
                await _writer.WriteLineAsync("var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);");
                await _writer.WriteAsync($"return impl.{operationId}(");
                foreach (var parameter in cSharpParameters)
                {
                    await _writer.WriteAsync($"{parameter}, ");
                }

                await _writer.WriteLineAsync("context);");
            }

            await _writer.WriteLineAsync("})");
            await _writer.WriteLineAsync($".WithName(\"{operation.OperationId}\");");
        }
    }

    private async Task WriteResultMethods()
    {
        var methods = new HashSet<string>();
        foreach (var (path, pathItem) in _document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.GetOperationsWithTag(_tag))
            {
                foreach (var (statusStr, response) in operation.Responses)
                {
                    if (!int.TryParse(statusStr, out int status)) continue;
                }
            }
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

    public static IEnumerable<KeyValuePair<OperationType, OpenApiOperation>> GetOperationsWithTag(this OpenApiPathItem pathItem, string tag) =>
        pathItem.Operations
            .Where(o => o.Value.Tags.Any(t => t.Name.Equals(tag)));
}

internal static class ParameterHelpers
{
    private static readonly Regex NonAlphaNumeric = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static string CSharpName(this OpenApiParameter parameter)
    {
        var name = NonAlphaNumeric.Replace(parameter.Name, string.Empty);
        if (name is not { Length: > 0 })
        {
            throw new InvalidOperationException($"Cannot get C# name from '{parameter.Name}'");
        }

        if (name.Length == 1) return name.ToLowerInvariant();
        if (char.IsLower(name[0])) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

internal static class StatusCodeHelper
{
    public static string GetMethod(int status) => status switch
    {
        200 => "Ok(object? obj = null) => Results.Ok(obj)",
        201 => "Created(Uri uri, object? obj = null) => Results.Created(uri, obj)",
        202 => "Accepted(Uri? uri = null, object? obj = null) => Results.Accepted(uri.ToString(), obj)",
        204 => "NoContent() => Results.NoContent()",
        301 => "MovedPermanently(Uri uri) => Results.Redirect(uri.ToString(), true, false)",
        302 => "Found(Uri uri) => Results.Redirect(uri.ToString(), false, false)",
        307 => "TemporaryRedirect(Uri uri) => Results.Redirect(uri.ToString(), false, true)",
        308 => "PermanentRedirect(Uri uri) => Results.Redirect(uri.ToString(), true, true)",
        400 => "BadRequest(object? errors = null) => Results.BadRequest(errors)",
        401 => "Unauthorized() => Results.Unauthorized()",
        402 => "PaymentRequired() => Results.StatusCode(402)",
        403 => "Forbidden() => Results.Forbid()",
        404 => "NotFound() => Results.NotFound()",
        405 => "MethodNotAllowed() => Results.StatusCode(405)",
        406 => "NotAcceptable() => Results.StatusCode(406)",
        409 => "Conflict(object? errors = null) => Results.Conflict(errors)",
        410 => "Gone() => Results.StatusCode(410)",
        411 => "LengthRequired() => Results.StatusCode(411)",
        412 => "PreconditionFailed() => Results.StatusCode(412)",
        415 => "UnsupportedMediaType() => Results.StatusCode(415)",
        416 => "RangeNotSatisfiable() => Results.StatusCode(416)",
        417 => "ExpectationFailed() => Results.StatusCode(417)",
        418 => "ImATeapot() => Results.StatusCode(418)",
        425 => "TooEarly() => Results.StatusCode(425)",
        428 => "PreconditionRequired() => Results.StatusCode(428)",
        429 => "TooManyRequests() => Results.StatusCode(429)",
        451 => "UnavailableForLegalReasons() => Results.StatusCode(451)",
        _ => $"Status{status}() => Results.StatusCode({status})",
    };
}