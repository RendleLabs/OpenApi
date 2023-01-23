using System.CodeDom.Compiler;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public class ApiBaseGenerator
{
    private readonly string _projectNamespace;
    private readonly OpenApiDocument _document;
    private readonly string _tag;

    private static readonly string[] Usings =
    {
        "System.Diagnostics.CodeAnalysis",
        "Microsoft.AspNetCore.Mvc",
        "ApiBase.Models",
    };

    private readonly IndentedTextWriter _writer;

    public ApiBaseGenerator(TextWriter writer, string projectNamespace, OpenApiDocument document, string tag)
    {
        _projectNamespace = projectNamespace;
        _document = document;
        _tag = tag;
        _writer = writer as IndentedTextWriter ?? new IndentedTextWriter(writer, "    ");
    }

    public async Task GenerateAsync()
    {
        await WriteUsings();
        await _writer.WriteLineNoTabsAsync();
        await _writer.WriteLineAsync($"namespace {_projectNamespace}.Api;");
        await _writer.WriteLineNoTabsAsync();
        await _writer.WriteLineAsync("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]");
        await _writer.WriteLineAsync($"public abstract partial class {_tag}Base");
        using (_writer.OpenBrace())
        {
            await _writer.WriteLineAsync("private static readonly IResult NotImplementedResult = Results.StatusCode(501);");
            await _writer.WriteLineNoTabsAsync();
            await _writer.WriteLineAsync($"private static void __Map<T>(WebApplication app, Func<IServiceProvider, T> builder) where T : {_tag}Base");
            
            using (_writer.OpenBrace())
            {
                foreach (var (path, pathItem) in _document.Paths.OrderBy(p => p.Key))
                {
                    await WritePathMaps(pathItem, path);
                }
            }

            await _writer.WriteLineNoTabsAsync();

            foreach (var resultType in ResultHelper.GetStatusCodes(_document, _tag))
            {
                var resultMethod = StatusCodeHelper.GetMethod(resultType.StatusCode, resultType.Type, resultType.IsArray);
                await _writer.WriteLineAsync($"protected static IResult {resultMethod};");
            }

            await _writer.WriteLineNoTabsAsync();

            await _writer.WriteLineAsync("public static LinkProvider Links { get; } = new LinkProvider();");

            await _writer.WriteLineNoTabsAsync();

            await _writer.WriteLineAsync("public readonly partial struct LinkProvider");
            using (_writer.OpenBrace())
            {
                foreach (var (path, pathItem) in _document.Paths.OrderBy(p => p.Key))
                {
                    await WriteLinks(pathItem, path);
                }
            }

            await _writer.WriteLineNoTabsAsync();

            foreach (var (path, pathItem) in _document.Paths.OrderBy(p => p.Key))
            {
                await WriteVirtualMethods(pathItem, path);
            }
        }

        await _writer.FlushAsync();
    }

    private async Task WritePathMaps(OpenApiPathItem pathItem, string path)
    {
        foreach (var (operationType, operation) in pathItem.GetOperationsWithTag(_tag).OrderBy(p => p.Key))
        {
            if (operation.OperationId is not { Length: > 0 } operationId)
            {
                continue;
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

            foreach (var parameter in pathItem.GetQueryParameters().Concat(operation.GetQueryParameters()))
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                cSharpParameters.Add(cSharpName = parameter.CSharpName());
                await _writer.WriteAsync($"[FromQuery] {type}? {cSharpName}, ");
            }

            foreach (var parameter in pathItem.GetHeaderParameters())
            {
                cSharpParameters.Add(cSharpName = parameter.CSharpName());
                await _writer.WriteAsync($"[FromHeader(Name = \"{parameter.Name}\"] string? {cSharpName}, ");
            }

            if (operation.RequestBody is { } requestBody)
            {
                foreach (var (_, mediaType) in requestBody.Content)
                {
                    if (mediaType.Schema.Title is { Length: > 0 } typeName)
                    {
                        var parameterName = typeName.ToCamelCase();
                        cSharpParameters.Add(parameterName);
                        await _writer.WriteAsync($"[FromBody] {typeName} {parameterName}, ");
                    }
                }
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
            await _writer.WriteLineNoTabsAsync();
        }
    }
    
    private async Task WriteVirtualMethods(OpenApiPathItem pathItem, string path)
    {
        foreach (var (operationType, operation) in pathItem.GetOperationsWithTag(_tag).OrderBy(p => p.Key))
        {
            if (operation.OperationId is not { Length: > 0 } operationId)
            {
                continue;
            }

            await _writer.WriteAsync($"protected virtual ValueTask<IResult> {operationId}(");
            string cSharpName;
            foreach (var parameter in pathItem.GetPathParameters())
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                await _writer.WriteAsync($"{type} {parameter.CSharpName()}, ");
            }

            foreach (var parameter in pathItem.GetQueryParameters().Concat(operation.GetQueryParameters()))
            {
                var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
                await _writer.WriteAsync($"{type}? {parameter.CSharpName()}, ");
            }

            foreach (var parameter in pathItem.GetHeaderParameters())
            {
                await _writer.WriteAsync($"string? {parameter.CSharpName()}, ");
            }

            if (operation.RequestBody is { } requestBody)
            {
                foreach (var (_, mediaType) in requestBody.Content)
                {
                    if (mediaType.Schema.Title is { Length: > 0 } typeName)
                    {
                        var parameterName = typeName.ToCamelCase();
                        await _writer.WriteAsync($"{typeName} {parameterName}, ");
                    }
                }
            }

            await _writer.WriteLineAsync("HttpContext context) => new(NotImplementedResult);");
        }
    }

    private async Task WriteLinks(OpenApiPathItem pathItem, string path)
    {
        foreach (var (operationType, operation) in pathItem.GetOperationsWithTag(_tag).OrderBy(p => p.Key))
        {
            if (operation.OperationId is not { Length: > 0 } operationId)
            {
                continue;
            }

            await _writer.WriteAsync($"public Uri {operationId}(");

            var pathParameters = pathItem.GetPathParameters().ToArray();
            var queryParameters = pathItem.GetQueryParameters().Concat(operation.GetQueryParameters()).ToArray();
            if (pathParameters.Length == 0 && queryParameters.Length == 0)
            {
                await _writer.WriteLineAsync($@") => new(""{path}"", UriKind.Relative);");
                continue;
            }
            await WriteMethodParameters(_writer, pathParameters);
            if (queryParameters.Length == 0)
            {
                await _writer.WriteLineAsync($@") => new($""{path}"", UriKind.Relative);");
                continue;
            }

            await WriteMethodParameters(_writer, queryParameters, true);

            await _writer.WriteAsync($@") => new($""{path}{{{operationId}QueryString(");

            await WriteCallParameters(_writer, queryParameters);

            await _writer.WriteLineAsync(")}\", UriKind.Relative);");
            
            await WriteQueryStringMethod(operationId, queryParameters);
        }
    }

    private async Task WriteQueryStringMethod(string operationId, OpenApiParameter[] queryParameters)
    {
        await _writer.WriteAsync($"private static string {operationId}QueryString(");
        await WriteMethodParameters(_writer, queryParameters, true);
        await _writer.WriteLineAsync(")");
        using (_writer.OpenBrace())
        {
            if (queryParameters.Length == 1)
            {
                await WriteQueryStringSingle(queryParameters[0]);
            }
            else
            {
                await WriteQueryStringMultiple(queryParameters);
            }
        }
    }

    private async Task WriteQueryStringMultiple(OpenApiParameter[] queryParameters)
    {
        await _writer.WriteAsync($"if ({queryParameters[0].CSharpName()} is null");

        for (int i = 1; i < queryParameters.Length; i++)
        {
            await _writer.WriteAsync($" && {queryParameters[0].CSharpName()} is null");
        }

        await _writer.WriteLineAsync(") return string.Empty;");

        await _writer.WriteLineAsync("var builder = new global::System.Text.StringBuilder();");
        foreach (var parameter in queryParameters)
        {
            await _writer.WriteLineNoTabsAsync();
            await _writer.WriteLineAsync($"if ({parameter.CSharpName()} is not null)");
            using (_writer.OpenBrace())
            {
                await _writer.WriteLineAsync("builder.Append(builder.Length == 0 ? '?' : '&');");

                if (parameter.Schema.Type == "string")
                {
                    await _writer.WriteAsync($"builder.AppendLine(\"{parameter.Name}={{Uri.EscapeDataString({parameter.CSharpName()})}}\");");
                }
                else
                {
                    await _writer.WriteAsync($"builder.AppendLine(\"{parameter.Name}={{{parameter.CSharpName()}}}\");");
                }
            }
        }

        await _writer.WriteLineNoTabsAsync();
        await _writer.WriteLineAsync("return builder.ToString();");
    }

    private async Task WriteQueryStringSingle(OpenApiParameter queryParameter)
    {
        await _writer.WriteLineAsync($"if ({queryParameter.CSharpName()} is null) return string.Empty;");

        if (queryParameter.Schema.Type == "string")
        {
            await _writer.WriteLineAsync($"return $\"?{queryParameter.Name}={{Uri.EscapeDataString({queryParameter.CSharpName()})}}\";");
        }
        else
        {
            await _writer.WriteLineAsync($"return $\"?{queryParameter.Name}={{{queryParameter.CSharpName()}}}\";");
        }
    }

    private static async Task WriteMethodParameters(TextWriter writer, IEnumerable<OpenApiParameter> parameters, bool nullable = false)
    {
        bool comma = false;
        foreach (var parameter in parameters)
        {
            var type = SchemaHelpers.SchemaTypeToDotNetType(parameter.Schema);
            var cSharpName = parameter.CSharpName();
            if (comma)
            {
                await writer.WriteAsync(", ");
            }
            else
            {
                comma = true;
            }

            if (nullable)
            {
                await writer.WriteAsync($"{type}? {cSharpName} = null");
            }
            else
            {
                await writer.WriteAsync($"{type} {cSharpName}");
            }
        }
    }

    private static async Task WriteCallParameters(TextWriter writer, IEnumerable<OpenApiParameter> parameters)
    {
        bool comma = false;
        foreach (var parameter in parameters)
        {
            var cSharpName = parameter.CSharpName();
            if (comma)
            {
                await writer.WriteAsync(", ");
            }
            else
            {
                comma = true;
            }

            await writer.WriteAsync(cSharpName);
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

internal static class NameHelper
{
    public static string ToCamelCase(this string original)
    {
        return string.IsNullOrEmpty(original)
            ? original
            : char.ToLowerInvariant(original[0]) + original[1..];
    }
}