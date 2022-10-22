namespace Testing.Api;

public class BookData
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<Guid, Book> _books = new();
    
    public async Task<Book> AddAsync(NewBook newBook)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = newBook.Title,
            Author = newBook.Author
        };

        await _semaphore.WaitAsync();

        try
        {
            _books.Add(book.Id, book);
        }
        finally
        {
            _semaphore.Release();
        }

        return book;
    }

    public async Task<Book> GetAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _books[id];
        }
        finally
        {
            _semaphore.Release();
        }
    }
}