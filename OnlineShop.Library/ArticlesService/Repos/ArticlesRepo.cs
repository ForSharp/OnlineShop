using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Common.Repos;
using OnlineShop.Library.Data.Migrations;

namespace OnlineShop.Library.ArticlesService.Repos;

public class ArticlesRepo : BaseRepo<Article>
{
    public ArticlesRepo(OrdersDbContext context) : base(context)
    {
        Table = Context.Articles;
    }
}