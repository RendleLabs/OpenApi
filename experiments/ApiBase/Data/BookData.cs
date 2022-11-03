using ApiBase.Models;

namespace ApiBase.Data;

public class BookData
{
    private readonly Dictionary<int, Book> _books = new()
    {
        [1] = new Book { Id = 1, Title = "Mort", Author = "Terry Pratchett", Year = 1987 },
        [42] = new Book { Id = 42, Title = "The Hitchhiker's Guide to the Galaxy", Author = "Douglas Adams", Year = 1979 },
    };

    private readonly object _mutex = new();

    public Book? Get(int id)
    {
        return _books.TryGetValue(id, out var book) ? book : null;
    }

    public Book Add(Book book)
    {
        lock (_mutex)
        {
            var id = _books.Keys.Max() + 1;
            book = new Book
            {
                Id = id,
                Title = book.Title,
                Author = book.Author,
                Year = book.Year
            };
            _books.Add(id, book);
        }

        return book;
    }
}