using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ApiBase.Models;
using Microsoft.Extensions.Primitives;

namespace ApiBase.Api;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
public abstract partial class BooksBase
{
    private static readonly IResult NotImplementedResult = Results.StatusCode(501);
    
    private static void __Map<T>(WebApplication app, Func<IServiceProvider, T> builder) where T : BooksBase
    {
        app.MapGet("/books/{id:int}", (int id, HttpContext context) =>
        {
            var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);
            return impl.Get(id, context);
        });

        app.MapPost("/books", ([FromBody] Book book, HttpContext context) =>
        {
            var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);
            return impl.Add(book, context);
        });
    }

    protected static IResult Ok(Book book) => Results.Ok(book);
    protected static IResult Created(Uri uri) => Results.Created(uri, null);
    protected static IResult NotFound() => Results.NotFound();

    public static LinkProvider Links { get; } = new LinkProvider();

    public readonly struct LinkProvider
    {
        public Uri Get(int id, string? format = null) => new($"/books/{id}{GetQueryString(format)}", UriKind.Relative);
        public Uri Add() => new Uri("/books", UriKind.Relative);

        private static string GetQueryString(string? format)
        {
            if (format is null) return string.Empty;
            var builder = new StringBuilder();
            if (format is not null)
            {
                if (builder.Length == 0)
                {
                    builder.Append('?');
                }
                else
                {
                    builder.Append('&');
                }

                builder.Append($"format={Uri.EscapeDataString(format)}");
            }

            return builder.ToString();
        }
    }
    
    protected virtual ValueTask<IResult> Get(int id, HttpContext context) => new(NotImplementedResult);

    protected virtual ValueTask<IResult> Add(Book book, HttpContext context) => new(NotImplementedResult);
}