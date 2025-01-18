using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Common.Repos;
using OnlineShop.Library.Data.Migrations;

namespace OnlineShop.Library.ArticlesService.Repos;

public class PriceListsRepo : BaseRepo<PriceList>
{
    public PriceListsRepo(OrdersDbContext context) : base(context)
    {
        Table = Context.PriceLists;
    }
}