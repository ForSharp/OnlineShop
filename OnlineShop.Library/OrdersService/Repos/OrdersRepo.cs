using Microsoft.EntityFrameworkCore;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Common.Interfaces;
using OnlineShop.Library.Common.Repos;
using OnlineShop.Library.Data.Migrations;
using OnlineShop.Library.OrdersService.Models;

namespace OnlineShop.Library.OrdersService.Repos;

public class OrdersRepo : BaseRepo<Order>
{
    private readonly IRepo<OrderedArticle> _orderedArticlesRepo;

    public OrdersRepo(IRepo<OrderedArticle> orderedArticlesRepo, OrdersDbContext context) : base(context)
    {
        _orderedArticlesRepo = orderedArticlesRepo;
        Table = Context.Orders;
    }

    public override async Task<IEnumerable<Order>> GetAllAsync() => await Table.Include(nameof(Order.Articles)).ToListAsync();

    public override async Task<Order> GetOneAsync(Guid id) => await Task.Run(() => Table.Include(nameof(Order.Articles)).FirstOrDefault(entity => entity.Id == id));
}