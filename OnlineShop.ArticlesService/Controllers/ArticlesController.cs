using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Common.Interfaces;
using OnlineShop.Library.Common.Repos;

namespace OnlineShop.ArticlesService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ArticlesController : RepoControllerBase<Article>
{
    public ArticlesController(IRepo<Article> articlesRepo, ILogger<ArticlesController> logger) : base(articlesRepo, logger)
    { }

    protected override void UpdateProperties(Article source, Article destination)
    {
        destination.Name = source.Name;
        destination.Description = source.Description;
    }
}