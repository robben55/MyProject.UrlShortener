using MediatR;
using UrlShortener.Entities;
using UrlShortener.Models;
using UrlShortener.Request;
using UrlShortener.Services;

namespace UrlShortener.Handlers;

public class GetShortUrlHandler : IRequestHandler<ShortenUrlRequest, IResult>
{
    private readonly ApplicationDbContext _context;
    private readonly UrlShorteningService _urlShorteningService;
    private readonly IHttpContextAccessor _accessor;
    public GetShortUrlHandler(ApplicationDbContext context,
        UrlShorteningService urlShorteningService, IHttpContextAccessor accessor)
    {
        _context = context;
        _urlShorteningService = urlShorteningService;
        _accessor = accessor;
    }

    public async Task<IResult> Handle(ShortenUrlRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return Results.BadRequest("The specified URL is invalid");
        }

        var code = await _urlShorteningService.GenerateUniqueCode();
        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            LongUrl = request.Url,
            Code = code,
            ShortUrl = $"{_accessor.HttpContext!.Request.Scheme}://{_accessor.HttpContext.Request.Host}/api/{code}",
            CreatedOnUtc = DateTime.UtcNow
        };

        await _context.ShortenedUrls.AddAsync(shortenedUrl, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Results.Ok(new
        {
            message = shortenedUrl.ShortUrl
        });
    }
}