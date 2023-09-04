using UrlShortener.Request;

namespace UrlShortener.Models;

public class ShortenUrlRequest : IHttpRequest
{
    public string Url { get; set; } = string.Empty;
    public int LimitOfRedirection { get; set; }
}