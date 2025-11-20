using BookStore.Api.Common.Mapping;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Create;
using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Features.Books.Overview;
using BookStore.Api.Features.Books.Delete;
using BookStore.Api.Features.Books.Get;
using BookStore.Api.Features.Books.Resolvers;
using BookStore.Api.Middleware;
using BookStore.Api.Validators;
using FluentValidation;
using BookStore.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite("Data Source=bookstore.db"));

builder.Services.AddScoped<IApplicationContext>(sp => sp.GetRequiredService<BookDbContext>());

// Add memory cache
builder.Services.AddMemoryCache();

builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddScoped<GetBookHandler>();

// Register AutoMapper resolvers
builder.Services.AddTransient<AuthorInitialsResolver>();
builder.Services.AddTransient<AvailabilityStatusResolver>();
builder.Services.AddTransient<CategoryDisplayResolver>();
builder.Services.AddTransient<DiscountedPriceResolver>();
builder.Services.AddTransient<PriceFormatterResolver>();
builder.Services.AddTransient<PublishedAgeResolver>();

// Register validator explicitly + scan assembly
builder.Services.AddScoped<IValidator<CreateBookProfileRequest>, CreateBookProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookProfileValidator>();

// Register both mapping profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<BookMappingProfile>();
    cfg.AddProfile<AdvancedBookMappingProfile>();
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "BookStore API",
            Version = "v1",
            Description = "API for managing books.",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com",
            }
        }
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    await context.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore API V1");
        c.RoutePrefix = string.Empty; // Serving Swagger UI at app root
        c.DisplayRequestDuration();
    });

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapPost("/books", async (CreateBookProfileRequest req, CreateBookHandler handler) =>
        await handler.Handle(req))
    .WithName("CreateBook")
    .WithSummary("Create a new book")
    .WithDescription("Creates a new book resource and returns the created advanced book profile DTO.")
    .WithTags("Books")
    .Produces<BookProfileDto>(StatusCodes.Status201Created)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest)
    .WithOpenApi();

app.MapGet("/books", async (GetAllBooksHandler handler) =>
        await handler.Handle(new GetAllBooksRequest()))
    .WithName("GetBooks")
    .WithSummary("List books")
    .WithDescription("Returns an overview collection of books.")
    .WithTags("Books")
    .WithOpenApi();

app.MapGet("/books/{id:guid}", async (Guid id, GetBookHandler handler) =>
        await handler.Handle(new GetBookRequest(id)))
    .WithName("GetBookById")
    .WithSummary("Get book by ID")
    .WithDescription("Returns the detailed advanced profile of a single book.")
    .WithTags("Books")
    .WithOpenApi();

app.MapDelete("/books/{id:guid}", async (Guid id, DeleteBookHandler handler) =>
        await handler.Handle(new DeleteBookRequest(id)))
    .WithName("DeleteBook")
    .WithSummary("Delete book")
    .WithDescription("Deletes a book by its unique identifier.")
    .WithTags("Books")
    .WithOpenApi();

app.Run();