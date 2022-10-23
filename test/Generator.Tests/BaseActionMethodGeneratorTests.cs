using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RendleLabs.OpenApi.Generator.Controllers;

namespace RendleLabs.OpenApi.Generator.Tests;

public class BaseActionMethodGeneratorTests
{
    private readonly OpenApiDocument _apiDocument;

    public BaseActionMethodGeneratorTests()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RendleLabs.OpenApi.Generator.Tests.openapi.yaml");
        _apiDocument = new OpenApiStreamReader().Read(stream, out _);
    }
    
    [Fact]
    public void GeneratesGetBooks()
    {
        var path = _apiDocument.Paths["/books"];
        var operation = path.Operations[OperationType.Get];
        var target = new BaseActionMethodGenerator("/books", OperationType.Get, operation);

        var writer = new Writer();
        target.Generate(writer.IndentedTextWriter);
        var actual = writer.ToString();
        Assert.Equal(Expected.GetBooks, actual);
    }

    [Fact]
    public void GeneratesGetBook()
    {
        var path = _apiDocument.Paths["/books/{id}"];
        var operation = path.Operations[OperationType.Get];
        var target = new BaseActionMethodGenerator("/books/{id}", OperationType.Get, operation);

        var writer = new Writer();
        target.Generate(writer.IndentedTextWriter);
        var actual = writer.ToString();
        Assert.Equal(Expected.GetBook, actual);
    }

    [Fact]
    public void GeneratesAddBook()
    {
        var path = _apiDocument.Paths["/books"];
        var operation = path.Operations[OperationType.Post];
        var target = new BaseActionMethodGenerator("/books", OperationType.Post, operation);

        var writer = new Writer();
        target.Generate(writer.IndentedTextWriter);
        var actual = writer.ToString();
        Assert.Equal(Expected.AddBook, actual);
    }

    private static class Expected
    {
        public const string GetBooks =
"""
[HttpGet("/books", Name = "GetBooks"]
public Task<ActionResult<List<Models.Book>>> GetBooks(CancellationToken cancellationToken) => Task.FromResult(StatusCode(501));

""";
        
        public const string GetBook =
"""
[HttpGet("/books/{id}", Name = "GetBook"]
public Task<ActionResult<Models.Book>> GetBook([FromRoute] int id, CancellationToken cancellationToken) => Task.FromResult(StatusCode(501));

""";
        
        public const string AddBook =
"""
[HttpPost("/books", Name = "AddBook"]
public Task<IActionResult> AddBook([FromBody] Models.NewBook newBook, CancellationToken cancellationToken) => Task.FromResult(StatusCode(501));

""";
    }
}