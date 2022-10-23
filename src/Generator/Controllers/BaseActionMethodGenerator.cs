using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.Controllers;

public class BaseActionMethodGenerator
{
    private readonly string _path;
    private readonly OperationType _operationType;
    private readonly OpenApiOperation _operation;

    public BaseActionMethodGenerator(string path, OperationType operationType, OpenApiOperation operation)
    {
        _path = path;
        _operationType = operationType;
        _operation = operation;
    }

    public void Generate(IndentedTextWriter writer)
    {
        writer.WriteLine($"[Http{_operationType}(\"{_path}\", Name = \"{_operation.OperationId}\"]");

        var model = GetModelName();
        var returnType = model is null ? "IActionResult" : $"ActionResult<{model}>";
        var parameters = GetParameters().ToArray();

        writer.Write($"public Task<{returnType}> {_operation.OperationId}(");
        if (parameters.Length > 0)
        {
            writer.Write(string.Join(", ", parameters.Select(p => p.ToString())));
            writer.Write(", ");
        }

        if (TryGetBodyName(out var bodyName, out var array))
        {
            writer.Write($"[FromBody] Models.{bodyName}");
            if (array)
            {
                writer.Write("[]");
            }

            writer.Write(' ');
            writer.Write(char.ToLowerInvariant(bodyName[0]));
            writer.Write(bodyName.AsSpan().Slice(1));
            writer.Write(", ");
        }

        writer.WriteLine("CancellationToken cancellationToken) => Task.FromResult(StatusCode(501));");
    }

    private bool TryGetBodyName([NotNullWhen(true)] out string? name, out bool array)
    {
        array = false;
        if (_operation.RequestBody?.Content is { Count: > 0 })
        {
            if (_operation.RequestBody.Content.TryGetValue("application/json", out var content))
            {
                if (content.Schema?.Type == "array")
                {
                    array = true;
                    if (content.Schema.Items?.Title is { Length: > 0 } title)
                    {
                        name = title;
                    }
                    else
                    {
                        name = $"{_operation.OperationId}RequestContent";
                    }
                }
                else
                {
                    if (content.Schema?.Title is { Length: > 0 } title)
                    {
                        name = title;
                    }
                    else
                    {
                        name = $"{_operation.OperationId}RequestContent";
                    }
                }

                return true;
            }
        }

        name = null;
        return false;
    }

    private string? GetModelName()
    {
        var anon = false;
        foreach (var (statusStr, response) in _operation.Responses)
        {
            if (response.Content is null) continue;

            if (statusStr is "200" or "201" or "202")
            {
                if (response.Content.TryGetValue("application/json", out var content))
                {
                    anon = true;
                    if (content.Schema.Type == "array" && content.Schema.Items.Title is { Length: > 0 })
                    {
                        return $"List<Models.{content.Schema.Items.Title}>";
                    }

                    if (content.Schema.Title is { Length: > 0 })
                    {
                        return $"Models.{content.Schema.Title}";
                    }
                }
            }
        }

        return anon ? $"Models.{_operation.OperationId}Model" : null;
    }

    private IEnumerable<ActionMethodParameter> GetParameters()
    {
        foreach (var apiParameter in _operation.Parameters)
        {
            var from = apiParameter.In switch
            {
                ParameterLocation.Query => "Query",
                ParameterLocation.Header => "Header",
                ParameterLocation.Path => "Route",
                _ => null
            };
            var type = apiParameter.Schema.Type.ToLower() switch
            {
                "string" => "string",
                "number" => "double",
                "integer" => apiParameter.Schema.Maximum.HasValue && apiParameter.Schema.Maximum.Value > int.MaxValue ? "long" : "int",
                "boolean" => "bool",
                _ => "object"
            };

            if (!apiParameter.Required)
            {
                type += "?";
            }

            yield return new ActionMethodParameter(from, type, apiParameter.Name);
        }
    }
}