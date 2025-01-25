using OnlineShop.Library.Common.Models;
using OnlineShop.Library.IdentityServer;
using OnlineShop.Library.Options;

namespace OnlineShop.Library.Clients.IdentityServer;

public interface IIdentityServerClient
{
    Task<Token> GetApiToken(IdentityServerApiOptions options);

    Task<Token> GetApiToken(IdentityServerUserNamePassword options);
}