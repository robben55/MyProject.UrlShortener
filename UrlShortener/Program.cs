using Microsoft.EntityFrameworkCore;
using UrlShortener;
using UrlShortener.Entities;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Services;
using Microsoft.Extensions.Caching.Memory;
using System;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<ApplicationDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<UrlShorteningService>(); // потому что инжектится еф коре который есть скопед


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.RoutePrefix = string.Empty;
    });

    app.ApplyMigrations();
}

app.MapPost("api/shorten", async (ShortenUrlRequest request,
        UrlShorteningService shorteningService,
        ApplicationDbContext applicationDbContext,
        HttpContext httpContext,
        IMemoryCache cache) =>
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

    cache.Set($"{shortenedUrl.Code}", shortenedUrl, TimeSpan.FromMinutes(2));

    await applicationDbContext.ShortenedUrls.AddAsync(shortenedUrl);

    await applicationDbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);

});


app.MapGet("api/{code}", async (string code,
    ApplicationDbContext dbContext,
    IMemoryCache cache) =>
{
    cache.TryGetValue(code, out ShortenedUrl? shortenedUrl);

    if (shortenedUrl is not null) return Results.Redirect(shortenedUrl.LongUrl);
    var url = await dbContext.ShortenedUrls
        .FirstOrDefaultAsync(s => s.Code == code);
    if (url is null)
    {
        return Results.NotFound();
    }

    cache.Set(code, url, TimeSpan.FromMinutes(2));
    return Results.Redirect(url.LongUrl);

});



app.UseHttpsRedirection();
app.Run();



