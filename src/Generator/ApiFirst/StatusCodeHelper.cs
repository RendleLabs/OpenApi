namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class StatusCodeHelper
{
    public static string GetMethod(int status, string? typeName, bool isArray)
    {
        string parameterName;
        if (typeName is { Length: > 0 })
        {
            parameterName = typeName.ToCamelCase();
            if (isArray)
            {
                parameterName += "s";
                typeName = $"IList<{typeName}>";
            }
        }
        else
        {
            typeName = "object";
            parameterName = "obj";
        }
        return status switch
        {
            200 => $"Ok({typeName}? {parameterName} = null) => Results.Ok({parameterName})",
            201 => $"Created(Uri uri, {typeName}? {parameterName} = null) => Results.Created(uri, {parameterName})",
            202 => $"Accepted(Uri? uri = null, {typeName}? {parameterName} = null) => Results.Accepted(uri.ToString(), {parameterName})",
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
}