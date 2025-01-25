using System.Text;
using AutoFixture;
using Newtonsoft.Json;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Constants;
using OnlineShop.Library.OrdersService.Models;

namespace OnlineShop.ApiService.ApiTests;

public class OrdersControllerTests : BaseRepoControllerTests
{
    public OrdersControllerTests() : base()
    {
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_order_THEN_it_is_being_added_to_database()
    {
        var expected = Fixture.Build<Order>()
            .With(o => o.Articles, Fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var jsonContent = JsonConvert.SerializeObject(expected);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Add}", httpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var getOneResponse = await SystemUnderTests.GetAsync($"orders?Id={expected.Id}");
        Assert.That(getOneResponse.IsSuccessStatusCode);
        var getOneResponseContent = await getOneResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<Order>(getOneResponseContent);
        AssertObjectsAreEqual(expected, actual);

        var jsonContentRemove = JsonConvert.SerializeObject(actual.Id);
        var httpContentRemove = new StringContent(jsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Remove}", httpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_several_orders_THEN_it_is_being_added_to_database()
    {
        var expected1 = Fixture.Build<Order>()
            .With(o => o.Articles, Fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var expected2 = Fixture.Build<Order>()
            .With(o => o.Articles, Fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var ordersToAdd = new[] { expected1, expected2 };
        var jsonContent = JsonConvert.SerializeObject(ordersToAdd);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addRangeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.AddRange}", httpContent);
        Assert.That(addRangeResponse.IsSuccessStatusCode);
        var addRangeResponseContent = await addRangeResponse.Content.ReadAsStringAsync();
        var addedOrderIds = JsonConvert.DeserializeObject<IEnumerable<Guid>>(addRangeResponseContent);

        var getAllResponse = await SystemUnderTests.GetAsync($"orders/{RepoActions.GetAll}");
        Assert.That(getAllResponse.IsSuccessStatusCode);
        var getOneResponseContent = await getAllResponse.Content.ReadAsStringAsync();
        var addedOrders = JsonConvert.DeserializeObject<IEnumerable<Order>>(getOneResponseContent);

        foreach (var orderId in addedOrderIds)
        {
            var expectedOrder = ordersToAdd.Single(o => o.Id == orderId);
            var actualOrder = addedOrders.Single(o => o.Id == orderId);
            AssertObjectsAreEqual(expectedOrder, actualOrder);
        }

        var jsonContentRemove = JsonConvert.SerializeObject(addedOrders.Select(order => order.Id));
        var httpContentRemove = new StringContent(jsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.RemoveRange}", httpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_update_order_THEN_it_is_being_update_in_database()
    {
        var orderedArticles = Fixture.CreateMany<OrderedArticle>().ToList();

        var expected = Fixture.Build<Order>()
            .With(o => o.Articles, orderedArticles)
            .Create();

        var jsonContent = JsonConvert.SerializeObject(expected);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Add}", httpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        orderedArticles.ForEach(oa => oa.Name = Fixture.Create<string>());

        expected.UserId = Fixture.Create<Guid>();
        expected.AddressId = Fixture.Create<Guid>();
        expected.Articles = orderedArticles;

        var jsonContentUpdate = JsonConvert.SerializeObject(expected);
        var httpContentUpdate = new StringContent(jsonContentUpdate, Encoding.UTF8, "application/json");
        var updateResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Update}", httpContentUpdate);
        Assert.That(updateResponse.IsSuccessStatusCode);
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<Order>(updateResponseContent);

        AssertObjectsAreEqual(expected, actual);

        var jsonContentRemove = JsonConvert.SerializeObject(actual.Id);
        var httpContentRemove = new StringContent(jsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Remove}", httpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    private void AssertObjectsAreEqual(Order expected, Order actual)
    {
        Assert.That(expected.Id, Is.EqualTo(actual.Id));
        Assert.That(expected.AddressId, Is.EqualTo(actual.AddressId));
        Assert.That(expected.UserId, Is.EqualTo(actual.UserId));
        Assert.That(expected.Created, Is.EqualTo(actual.Created));
        Assert.That(expected.Articles.Count(), Is.EqualTo(actual.Articles.Count()));
    }
}