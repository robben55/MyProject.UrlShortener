using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Request;

public class CodeRequest: IHttpRequest
{
    public required string Code { get; set; }
}