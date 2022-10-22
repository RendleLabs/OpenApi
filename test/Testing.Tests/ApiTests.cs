using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testing.Api;

namespace RendleLabs.OpenApi.Testing.Tests;

public class ApiTests
{
    [Fact]
    public async Task AddsAndGetsBook()
    {
        await using var appFactory = new WebApplicationFactory<BookData>();
        using var client = appFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/books")
        {
            Content = JsonContent.Create(new { Title = "Mort", Author = "Terry Pratchett" }),
        };
        var response = await client.SendAsync(request);
        Assert.True(response.IsSuccessStatusCode);
        var location = response.Headers.Location;
        var book = await client.GetFromJsonAsync<Book>(location);
        Assert.NotNull(book);
        Assert.Equal("Mort", book.Title);
        Assert.Equal("Terry Pratchett", book.Author);
    }

    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}