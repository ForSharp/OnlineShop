using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Library.UserManagementService.Responses;

public class IdentityResultDto
{
    public bool Succeeded { get; set; }

    public IEnumerable<IdentityError> Errors { get; set; }
}