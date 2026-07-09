using Microsoft.AspNetCore.Http;

namespace Synkan.Application.Interfaces;

public interface IAuthCookieService
{
    void SetAuthCookie(HttpResponse response, string token);
}