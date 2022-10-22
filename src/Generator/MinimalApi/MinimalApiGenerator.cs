using System.CodeDom.Compiler;
using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Generator.MinimalApi;

public class MinimalApiGenerator
{
    private readonly string _openApiFile;
    private readonly string _output;

    private MinimalApiGenerator(string openApiFile, string? output)
    {
        _openApiFile = openApiFile;
        if (output is not { Length: > 0 })
        {
            output = Path.GetFileNameWithoutExtension(openApiFile);
        }
        _output = Path.GetFullPath(output);
    }

    private async Task<int> InvokeAsync()
    {
        var (document, diagnostic) = await LoadDocumentAsync();

        if (diagnostic.Errors is { Count: > 0 } errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"{error.Message} at {error.Pointer}");
            }

            return 1;
        }
        
        Directory.CreateDirectory(_output);
        await CreateProjectFile();
        await CreateProgramFile(document);

        return 0;
    }

    private async Task<(OpenApiDocument, OpenApiDiagnostic)> LoadDocumentAsync()
    {
        await using var stream = File.OpenRead(_openApiFile);
        var result = await new OpenApiStreamReader().ReadAsync(stream);
        return (result.OpenApiDocument, result.OpenApiDiagnostic);
    }

    private async Task CreateProjectFile()
    {
        var name = $"{Path.GetFileNameWithoutExtension(_output)}.csproj";
        var path = Path.Combine(_output, name);
        await using var writeStream = File.Create(path);
        await using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RendleLabs.OpenApi.Generator.MinimalApi.Project.xml");
        Debug.Assert(resourceStream != null);
        await resourceStream.CopyToAsync(writeStream);
    }

    private async Task CreateProgramFile(OpenApiDocument openApiDocument)
    {
        var path = Path.Combine("_output", "Program.cs");
        await File.WriteAllTextAsync(path, GenerateProgram(openApiDocument));
    }

    private string GenerateProgram(OpenApiDocument openApiDocument)
    {
        var builder = new StringBuilder();
        using var stringWriter = new StringWriter(builder);
        using var writer = new IndentedTextWriter(stringWriter, "    ");

        writer.WriteLine("var builder = WebApplication.CreateBuilder(args);");
        writer.WriteLine();
        writer.WriteLine("var app = builder.Build();");
        writer.WriteLine();

        foreach (var (pathKey, pathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                writer.WriteLine($"app.Map{operationType}(\"{pathKey}\", async (context) =>");
                using (writer.OpenIndent())
                {
                    using (writer.OpenBrace())
                    {
                        
                    }
                }
                writer.Indent++;
                writer.WriteLine("{");
            }
        }
        
        writer.WriteLine("app.Run();");
        writer.Flush();
        return builder.ToString();
    }

    public static Command CreateCommand()
    {
        var command = new Command("minimal");

        var openApiFileArgument = new Argument<string>("OpenApiFile");
        var outputOption = new Option<string>(new[] { "--output", "-o" }, () => string.Empty, "Output directory");
        
        command.SetHandler(async (context) =>
        {
            var openApiFile = context.ParseResult.GetValueForArgument(openApiFileArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            context.ExitCode = await new MinimalApiGenerator(openApiFile, output).InvokeAsync();
        });
        
        command.SetHandler(
            (openApiFile, output) => new MinimalApiGenerator(openApiFile, output).InvokeAsync(),
            openApiFileArgument,
            outputOption);
        
        return command;
    }
}