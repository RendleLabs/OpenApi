using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Bundle;

internal static class OpenApiDiagnosticWrite
{
    public static void Write(this OpenApiDiagnostic diagnostic)
    {
        if (diagnostic.Warnings is { Count: > 0 } warnings)
        {
            WriteWarnings(warnings);
        }

        if (diagnostic.Errors is { Count: > 0 } errors)
        {
            WriteErrors(errors);
        }
    }
    
    private static void WriteErrors(IList<OpenApiError> errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"ERROR: [{error.Pointer}] {error.Message}");
        }
    }

    private static void WriteWarnings(IList<OpenApiError> warnings)
    {
        foreach (var warning in warnings)
        {
            Console.WriteLine($"WARNING: [{warning.Pointer}] {warning.Message}");
        }
    }
}