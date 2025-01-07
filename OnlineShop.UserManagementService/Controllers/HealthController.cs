using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.UserManagementService.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class HealthController: ControllerBase
{
    [HttpGet]
    public string Check() => "Is alive";
}