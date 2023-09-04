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

        
        switch (shortenedUrl)
        {
            case null:
                return Results.NotFound();
            case { NumberOfRedirection: 0, LimitOfRedirection: 0 }:
                return Results.Redirect(shortenedUrl.LongUrl);
        }

        

        if (shortenedUrl!.NumberOfRedirection >= shortenedUrl.LimitOfRedirection ||  shortenedUrl.NumberOfRedirection == shortenedUrl.LimitOfRedirection)
            return Results.Problem(title: "Out of redirections limit.", detail: "user created this link set up limit for redirection. sorry!"); // true
        {
            await _context.ShortenedUrls.ExecuteUpdateAsync(s =>
                s.SetProperty(s => s.NumberOfRedirection, s => s.NumberOfRedirection + 1), cancellationToken);

            return Results.Redirect(shortenedUrl.LongUrl);
        }//false

    }
}
