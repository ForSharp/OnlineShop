using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Common.Repos;
using OnlineShop.Library.Data.Migrations;

namespace OnlineShop.Library.OrdersService.Repos;

public class OrderedArticlesRepo : BaseRepo<OrderedArticle>
{
    public OrderedArticlesRepo(OrdersDbContext context) : base(context)
    {
        Table = Context.OrderedArticles;
    }
}