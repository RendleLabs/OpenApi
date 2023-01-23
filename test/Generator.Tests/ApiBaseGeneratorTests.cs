using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RendleLabs.OpenApi.Generator.ApiFirst;

namespace RendleLabs.OpenApi.Generator.Tests;

public class ApiBaseGeneratorTests
{
    private readonly OpenApiDocument _apiDocument;

    public ApiBaseGeneratorTests()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RendleLabs.OpenApi.Generator.Tests.openapi.yaml");
        _apiDocument = new OpenApiStreamReader().Read(stream, out _);
    }
    
    [Fact]
    public async Task GeneratesBaseClass()
    {
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);
        var target = new ApiBaseGenerator(writer, "ApiBase", _apiDocument, "Books");
        await target.GenerateAsync();
        await writer.FlushAsync();
        var actual = builder.ToString();
        Assert.Equal(ExpectedBooksBase.Trim(), actual.Trim());
    }

    private const string ExpectedBooksBase = @"using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using ApiBase.Models;

namespace ApiBase.Api;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
public abstract partial class BooksBase
{
    private static readonly IResult NotImplementedResult = Results.StatusCode(501);

    private static void __Map<T>(WebApplication app, Func<IServiceProvider, T> builder) where T : BooksBase
    {
        app.MapGet(""/books"", ([FromQuery] int? skip, [FromQuery] int? take, HttpContext context) =>
        {
            var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);
            return impl.GetBooks(skip, take, context);
        })
        .WithName(""GetBooks"");

        app.MapPost(""/books"", ([FromBody] NewBook newBook, HttpContext context) =>
        {
            var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);
            return impl.AddBook(newBook, context);
        })
        .WithName(""AddBook"");

        app.MapGet(""/books/{id}"", (int id, HttpContext context) =>
        {
            var impl = context.RequestServices.GetService<T>() ?? builder(context.RequestServices);
            return impl.GetBook(id, context);
        })
        .WithName(""GetBook"");

    }

    protected static IResult Ok(IList<Book>? books = null) => Results.Ok(books);
    protected static IResult Ok(Book? book = null) => Results.Ok(book);
    protected static IResult Created(Uri uri, Book? book = null) => Results.Created(uri, book);
    protected static IResult NotFound() => Results.NotFound();

    public static LinkProvider Links { get; } = new LinkProvider();

    public readonly partial struct LinkProvider
    {
        public Uri GetBooks(int? skip = null, int? take = null) => new($""/books{GetBooksQueryString(skip, take)}"", UriKind.Relative);
        private static string GetBooksQueryString(int? skip = null, int? take = null)
        {
            if (skip is null && skip is null) return string.Empty;
            var builder = new global::System.Text.StringBuilder();

            if (skip is not null)
            {
                builder.Append(builder.Length == 0 ? '?' : '&');
                builder.AppendLine(""skip={skip}"");}

            if (take is not null)
            {
                builder.Append(builder.Length == 0 ? '?' : '&');
                builder.AppendLine(""take={take}"");}

            return builder.ToString();
        }
        public Uri AddBook() => new(""/books"", UriKind.Relative);
        public Uri GetBook(int id) => new($""/books/{id}"", UriKind.Relative);
    }

    protected virtual ValueTask<IResult> GetBooks(int? skip, int? take, HttpContext context) => new(NotImplementedResult);
    protected virtual ValueTask<IResult> AddBook(NewBook newBook, HttpContext context) => new(NotImplementedResult);
    protected virtual ValueTask<IResult> GetBook(int id, HttpContext context) => new(NotImplementedResult);
}";
}