using System.CodeDom.Compiler;
using System.Text;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.Controllers;

public class BaseControllerGenerator
{
    private readonly OpenApiDocument _apiDocument;
    private readonly string _ns;

    public BaseControllerGenerator(OpenApiDocument apiDocument, string ns)
    {
        _ns = ns;
        _apiDocument = apiDocument;
    }

    private void GenerateController(string ns, string tag)
    {
        var builder = new StringBuilder();
        using var stringWriter = new StringWriter(builder);
        using var writer = new IndentedTextWriter(stringWriter, "    ");
        
        writer.WriteLine("using Microsoft.AspNetCore.Mvc;");
        writer.WriteLine();
        writer.WriteLine($"namespace {_ns};");
        writer.WriteLine();

        var name = $"{tag}ControllerBase";
        
        writer.WriteLine($"[ApiController]");
        writer.WriteLine($"public abstract class {name}: ControllerBase");
        using (writer.OpenBrace())
        {
            writer.WriteLine("private readonly ILogger _logger;");
            writer.WriteLine();
            writer.WriteLine($"protected {name}(ILogger logger)");
            using (writer.OpenBrace())
            {
                writer.WriteLine("_logger = logger;");
            }
        }
    }
}