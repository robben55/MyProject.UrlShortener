using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Request;

namespace UrlShortener.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication GetLongUrl<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapGet(template, async (IMediator send, [AsParameters] TRequest request) => await send.Send(request));
        return app;
    }


    public static WebApplication MakeUrlShort<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapPost(template, async (IMediator send, [FromBody] TRequest request) => await send.Send(request));
        return app;
    }

}