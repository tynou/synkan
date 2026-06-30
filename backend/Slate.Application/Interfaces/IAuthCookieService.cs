using Microsoft.AspNetCore.Http;

namespace Slate.Application.Interfaces;

public interface IAuthCookieService
{
    void SetAuthCookie(HttpResponse response, string token);
}