using MediatR;

namespace UrlShortener.Request;

public interface IHttpRequest : IRequest<IResult>
{
    
}