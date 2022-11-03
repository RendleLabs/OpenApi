using ApiBase.Data;
using ApiBase.Models;

namespace ApiBase.Api;

public sealed class Books : BooksBase
{
    private readonly BookData _data;
    private readonly ILogger<Books> _logger;

    public Books(BookData data, ILogger<Books> logger)
    {
        _data = data;
        _logger = logger;
    }

    protected override ValueTask<IResult> Get(int id, HttpContext context)
    {
        try
        {
            var book = _data.Get(id);
            return new ValueTask<IResult>(book is null
                ? NotFound()
                : Ok(book));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Books.Get failed");
            throw;
        }
    }

    protected override ValueTask<IResult> Add(Book book, HttpContext context)
    {
        book = _data.Add(book);
        return new ValueTask<IResult>(Created(Links.Get(book.Id)));
    }
}