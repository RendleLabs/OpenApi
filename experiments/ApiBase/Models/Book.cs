namespace ApiBase.Models;

public partial class Book
{
    public int Id { get; init; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
}