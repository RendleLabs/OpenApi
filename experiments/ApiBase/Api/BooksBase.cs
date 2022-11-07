using System.Diagnostics.CodeAnalysis;
using ApiBase.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Api;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
public abstract class BooksBase
{
    private static readonly IResult NotImplementedResult = Results.StatusCode(501);
    private static readonly IResult NotFoundResult = Results.NotFound();
    
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
    protected static IResult NotFound() => NotFoundResult;

    public static LinkProvider Links { get; } = new LinkProvider();

    public readonly struct LinkProvider
    {
        public Uri Get(int id) => new($"/books/{id}", UriKind.Relative);
        public Uri Add() => new Uri("/books", UriKind.Relative);
    }
    
    protected virtual ValueTask<IResult> Get(int id, HttpContext context) => new(NotImplementedResult);

    protected virtual ValueTask<IResult> Add(Book book, HttpContext context) => new(NotImplementedResult);
}