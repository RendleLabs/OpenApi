﻿using Microsoft.AspNetCore.Builder;

namespace DotLabs.OpenApi.Web;

internal static class EndpointConventionBuilderExtensions
{
    public static IEndpointConventionBuilder AllowAnonymous(this IEndpointConventionBuilder builder, bool allowAnonymous)
    {
        if (allowAnonymous)
        {
            builder.AllowAnonymous();
        }

        return builder;
    }
}