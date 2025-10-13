using BookStore.Api.Data;
using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Features.Books.Overview;
using BookStore.Api.Features.Books.Delete;
using BookStore.Api.Features.Books.Get;
using BookStore.Api.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite("Data Source=bookstore.db"));

builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddScoped<GetBookHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/books", async (CreateBookRequest req, CreateBookHandler handler) => 
    await handler.Handle(req));

app.MapGet("/books", async (GetAllBooksHandler handler) =>
    await handler.Handle(new  GetAllBooksRequest()));

app.MapGet("/books/{id:guid}", async (Guid id, GetBookHandler handler) =>
    await handler.Handle(new GetBookRequest(id)));

app.MapDelete("/books/{id:guid}", async (Guid id, DeleteBookHandler handler) =>
    await handler.Handle(new DeleteBookRequest(id)));

app.Run();