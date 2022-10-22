using Testing.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BookData>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/books/{id}", async (Guid id, BookData bookData) =>
    {
        try
        {
            return Results.Ok(await bookData.GetAsync(id));
        }
        catch (IndexOutOfRangeException)
        {
            return Results.NotFound();
        }
    })
    .WithName("GetBook");

app.MapPost("/books", async (NewBook newBook, BookData bookData) =>
    {
        var book = await bookData.AddAsync(newBook);
        return Results.CreatedAtRoute("GetBook", new { id = book.Id });
    })
    .WithName("AddBook");


app.Run();