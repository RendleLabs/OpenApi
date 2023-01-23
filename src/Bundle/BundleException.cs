using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Bundle;

public class BundleException : Exception
{
    public BundleException(string message) : base(message)
    {
    }

    public BundleException(string message, Exception inner) : base(message, inner)
    {
    }

    public BundleException(string message, OpenApiDiagnostic diagnostic) : base(message)
    {
        Diagnostic = diagnostic;
    }

    public BundleException(string message, OpenApiDiagnostic diagnostic, Exception inner) : base(message, inner)
    {
        Diagnostic = diagnostic;
    }

    public OpenApiDiagnostic? Diagnostic { get; }
}