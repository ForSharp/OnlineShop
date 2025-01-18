using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.OrdersService.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class HealthCheckController
{
    [HttpGet]
    public string Check() => "Service is online";
}