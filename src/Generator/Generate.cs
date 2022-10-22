using System.CommandLine;
using RendleLabs.OpenApi.Generator.MinimalApi;

namespace RendleLabs.OpenApi.Generator;

public class Generate
{
    public static Command CreateCommand()
    {
        var command = new Command("generate", "Generate an API project from an OpenAPI spec.");
        command.AddAlias("gen");
        command.AddAlias("g");
        command.Add(MinimalApiGenerator.CreateCommand());
        return command;
    }
}