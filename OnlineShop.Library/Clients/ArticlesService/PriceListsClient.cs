using Microsoft.Extensions.Options;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Options;

namespace OnlineShop.Library.Clients.ArticlesService;

public class PriceListsClient : RepoClientBase<PriceList>
{
    public PriceListsClient(HttpClient client, IOptions<ServiceAddressOptions> options) : base(client, options)
    { }

    protected override void InitializeClient(IOptions<ServiceAddressOptions> options)
    {
        HttpClient.BaseAddress = new Uri(options.Value.ArticlesService);
    }

    protected override void SetControllerName()
    {
        ControllerName = "PriceLists";
    }
}