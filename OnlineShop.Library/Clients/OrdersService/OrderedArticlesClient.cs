using Microsoft.Extensions.Options;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Options;

namespace OnlineShop.Library.Clients.OrdersService;

public class OrderedArticlesClient : RepoClientBase<OrderedArticle>
{
    public OrderedArticlesClient(HttpClient client, IOptions<ServiceAddressOptions> options) : base(client, options)
    { }

    protected override void InitializeClient(IOptions<ServiceAddressOptions> options)
    {
        HttpClient.BaseAddress = new Uri(options.Value.OrdersService);
    }

    protected override void SetControllerName()
    {
        ControllerName = "OrderedArticles";
    }
}