using System.CommandLine;

var rootCommand = new RootCommand();

var generateCommand = new Command("generate", "Generate an API project");
generateCommand.AddAlias("gen");
generateCommand.AddAlias("g");
var openApiFileNameArgument = new Argument<string>("openApiFileName", "The OpenAPI definition to generate code for.");
generateCommand.AddArgument(openApiFileNameArgument);
var outputOption = new Option<string>(new[]{"--output", "-o"}, "Output directory.");
generateCommand.AddOption(outputOption);

generateCommand.SetHandler((openApiFileName, output) =>
{
    Console.WriteLine(openApiFileName);
    Console.WriteLine(output);
}, openApiFileNameArgument, outputOption);

rootCommand.Add(generateCommand);

return await rootCommand.InvokeAsync(args);
