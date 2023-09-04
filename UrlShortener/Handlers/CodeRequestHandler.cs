using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Request;

namespace UrlShortener.Handlers;

public class CodeRequestHandler : IRequestHandler<CodeRequest, IResult>
{
    private readonly ApplicationDbContext _context;

    public CodeRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(CodeRequest request,
        CancellationToken cancellationToken)
    {
        var shortenedUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(s => s.Code == request.Code, cancellationToken);

        return shortenedUrl is null ? Results.NotFound() : Results.Redirect(shortenedUrl.LongUrl);

    }
}
