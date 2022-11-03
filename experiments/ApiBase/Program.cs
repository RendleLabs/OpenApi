using ApiBase.Api;
using ApiBase.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BookData>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapApi<Books>();

app.Run();
