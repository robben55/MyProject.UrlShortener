using Microsoft.EntityFrameworkCore;
using UrlShortener;
using UrlShortener.Entities;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using UrlShortener.Request;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDbContext<ApplicationDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
    });

    app.ApplyMigrations();
}

app.MapPost("api/shorten", async (ShortenUrlRequest request,
        UrlShorteningService shorteningService,
        ApplicationDbContext applicationDbContext,
        HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The specified URL is invalid.");
    }

    var code = await shorteningService.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl
    {
        Id = Guid.NewGuid(),
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
        CreatedOnUtc = DateTime.UtcNow
    };

    await applicationDbContext.ShortenedUrls.AddAsync(shortenedUrl);

    await applicationDbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);

});

app.GetLongUrl<CodeRequest>("/api/{code}");
app.UseHttpsRedirection();
app.Run();