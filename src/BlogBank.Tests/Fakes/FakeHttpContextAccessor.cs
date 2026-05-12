using Microsoft.AspNetCore.Http;

namespace BlogBank.Tests.Fakes;

public class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = null;
}