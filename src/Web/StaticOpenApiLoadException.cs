using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Web;

[Serializable]
public class StaticOpenApiLoadException : Exception
{
    public StaticOpenApiLoadException(IList<OpenApiError> errors) : base("Error loading OpenApi document")
    {
        Errors = errors;
    }

    public IList<OpenApiError> Errors { get; }
}