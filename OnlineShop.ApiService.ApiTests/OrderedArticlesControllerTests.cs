﻿using System.Text;
using AutoFixture;
using Newtonsoft.Json;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Constants;
using OnlineShop.Library.OrdersService.Models;

namespace OnlineShop.ApiService.ApiTests;

public class OrderedArticlesControllerTests : BaseRepoControllerTests
{
    public OrderedArticlesControllerTests() : base()
    {
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_order_THEN_it_is_being_added_to_database()
    {
        var order = Fixture.Build<Order>()
            .With(o => o.Articles, Enumerable.Empty<OrderedArticle>().ToList())
            .Create();

        var jsonContent = JsonConvert.SerializeObject(order);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Add}", httpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var expected = Fixture.Build<OrderedArticle>()
            .With(oa => oa.Order, order)
            .With(oa => oa.OrderId, order.Id)
            .Create();

        var orderedArticleJsonContent = JsonConvert.SerializeObject(expected);
        var orderedArticleHttpContent = new StringContent(orderedArticleJsonContent, Encoding.UTF8, "application/json");
        var orderedArticleAddResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.Add}", orderedArticleHttpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var orderedArticleAddResponseContent = await orderedArticleAddResponse.Content.ReadAsStringAsync();
        var orderedArticleId = JsonConvert.DeserializeObject<Guid>(orderedArticleAddResponseContent);

        var getOneResponse = await SystemUnderTests.GetAsync($"orderedarticles?Id={orderedArticleId}");
        Assert.That(getOneResponse.IsSuccessStatusCode);
        var getOneResponseContent = await getOneResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<OrderedArticle>(getOneResponseContent);
        AssertObjectsAreEqual(expected, actual);

        var orderJsonContentRemove = JsonConvert.SerializeObject(order.Id);
        var orderHttpContentRemove = new StringContent(orderJsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Remove}", orderHttpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_several_orders_THEN_it_is_being_added_to_database()
    {
        var order = Fixture.Build<Order>()
            .With(o => o.Articles, Enumerable.Empty<OrderedArticle>().ToList())
            .Create();

        var jsonContent = JsonConvert.SerializeObject(order);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Add}", httpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var expected1 = Fixture.Build<OrderedArticle>()
            .With(oa => oa.Order, order)
            .With(oa => oa.OrderId, order.Id)
            .Create();

        var expected2 = Fixture.Build<OrderedArticle>()
            .With(oa => oa.Order, order)
            .With(oa => oa.OrderId, order.Id)
            .Create();

        var orderedArticlesToAdd = new[] { expected1, expected2 };

        var orderedArticlesJsonContent = JsonConvert.SerializeObject(orderedArticlesToAdd);
        var orderedArticlesHttpContent =
            new StringContent(orderedArticlesJsonContent, Encoding.UTF8, "application/json");
        var orderedArticlesAddRangeResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.AddRange}", orderedArticlesHttpContent);
        Assert.That(orderedArticlesAddRangeResponse.IsSuccessStatusCode);

        var addRangeResponseContent = await orderedArticlesAddRangeResponse.Content.ReadAsStringAsync();
        var addedOrderIds = JsonConvert.DeserializeObject<IEnumerable<Guid>>(addRangeResponseContent);

        var getAllResponse = await SystemUnderTests.GetAsync($"orderedarticles/{RepoActions.GetAll}");
        Assert.That(getAllResponse.IsSuccessStatusCode);

        var getOneResponseContent = await getAllResponse.Content.ReadAsStringAsync();
        var addedOrderedArticles = JsonConvert.DeserializeObject<IEnumerable<OrderedArticle>>(getOneResponseContent);

        foreach (var orderedArticleId in addedOrderIds)
        {
            var expectedOrder = orderedArticlesToAdd.Single(o => o.Id == orderedArticleId);
            var actualOrder = addedOrderedArticles.Single(o => o.Id == orderedArticleId);
            AssertObjectsAreEqual(expectedOrder, actualOrder);
        }

        var orderedArticlesJsonContentRemove = JsonConvert.SerializeObject(addedOrderedArticles.Select(oa => oa.Id));
        var orderedArticlesHttpContentRemove =
            new StringContent(orderedArticlesJsonContentRemove, Encoding.UTF8, "application/json");
        var orderedArticlesRemoveRangeResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.RemoveRange}",
                orderedArticlesHttpContentRemove);
        Assert.That(orderedArticlesRemoveRangeResponse.IsSuccessStatusCode);

        var jsonContentRemove = JsonConvert.SerializeObject(order.Id);
        var httpContentRemove = new StringContent(jsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Remove}", httpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_update_order_THEN_it_is_being_update_in_database()
    {
        var order = Fixture.Build<Order>()
            .With(o => o.Articles, Enumerable.Empty<OrderedArticle>().ToList())
            .Create();

        var jsonContent = JsonConvert.SerializeObject(order);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var addResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Add}", httpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var expected = Fixture.Build<OrderedArticle>()
            .With(oa => oa.Order, order)
            .With(oa => oa.OrderId, order.Id)
            .Create();

        var orderedArticleJsonContent = JsonConvert.SerializeObject(expected);
        var orderedArticleHttpContent = new StringContent(orderedArticleJsonContent, Encoding.UTF8, "application/json");
        var orderedArticleAddResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.Add}", orderedArticleHttpContent);
        Assert.That(addResponse.IsSuccessStatusCode);

        var orderedArticleAddResponseContent = await orderedArticleAddResponse.Content.ReadAsStringAsync();
        var orderedArticleId = JsonConvert.DeserializeObject<Guid>(orderedArticleAddResponseContent);

        expected.Name = Fixture.Create<string>();
        expected.Description = Fixture.Create<string>();
        expected.Price = Fixture.Create<decimal>();
        expected.Quantity = Fixture.Create<int>();

        var jsonContentUpdate = JsonConvert.SerializeObject(expected);
        var httpContentUpdate = new StringContent(jsonContentUpdate, Encoding.UTF8, "application/json");
        var updateResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.Update}", httpContentUpdate);
        Assert.That(updateResponse.IsSuccessStatusCode);
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var actual = JsonConvert.DeserializeObject<OrderedArticle>(updateResponseContent);

        AssertObjectsAreEqual(expected, actual);

        var orderedArticlesJsonContentRemove = JsonConvert.SerializeObject(expected.Id);
        var orderedArticlesHttpContentRemove =
            new StringContent(orderedArticlesJsonContentRemove, Encoding.UTF8, "application/json");
        var orderedArticlesRemoveRangeResponse =
            await SystemUnderTests.PostAsync($"orderedarticles/{RepoActions.Remove}", orderedArticlesHttpContentRemove);
        Assert.That(orderedArticlesRemoveRangeResponse.IsSuccessStatusCode);

        var orderJsonContentRemove = JsonConvert.SerializeObject(order.Id);
        var orderHttpContentRemove = new StringContent(orderJsonContentRemove, Encoding.UTF8, "application/json");
        var removeResponse = await SystemUnderTests.PostAsync($"orders/{RepoActions.Remove}", orderHttpContentRemove);
        Assert.That(removeResponse.IsSuccessStatusCode);
    }

    private void AssertObjectsAreEqual(OrderedArticle expected, OrderedArticle actual)
    {
        Assert.That(expected.Id, Is.EqualTo(actual.Id));
        Assert.That(expected.Name, Is.EqualTo(actual.Name));
        Assert.That(expected.Description, Is.EqualTo(actual.Description));
        Assert.That(expected.Price, Is.EqualTo(actual.Price));
        Assert.That(expected.Quantity, Is.EqualTo(actual.Quantity));

        if (expected.Price != actual.Price)
        {
            expected.PriceListName = "Manualy assigned";
        }
    }
}