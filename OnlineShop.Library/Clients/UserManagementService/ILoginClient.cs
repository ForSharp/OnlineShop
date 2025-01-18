using OnlineShop.Library.Common.Models;
using OnlineShop.Library.Options;
using OnlineShop.Library.UserManagementService.Models;

namespace OnlineShop.Library.Clients.UserManagementService;

public interface ILoginClient: IDisposable
{
    Task<UserManagementServiceToken> GetApiTokenByClientSeceret(IdentityServerApiOptions options);

    Task<UserManagementServiceToken> GetApiTokenByUsernameAndPassword(IdentityServerUserNamePassword options);
}