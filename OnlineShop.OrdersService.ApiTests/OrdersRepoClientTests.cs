using AutoFixture;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Moq;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.Clients.OrdersService;
using OnlineShop.Library.Clients.UserManagementService;
using OnlineShop.Library.Options;
using OnlineShop.Library.OrdersService.Models;

namespace OnlineShop.OrdersService.ApiTests;

public class OrdersRepoClientTests
{
    private readonly Fixture _fixture = new Fixture();
    private ILoginClient _loginClient;
    private OrdersClient _systemUnderTests;

    public OrdersRepoClientTests()
    {
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [SetUp]
    public async Task Setup()
    {
        var serviceAdressOptionsMock = new Mock<IOptions<ServiceAddressOptions>>();

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        switch (env)
        {
            case "Docker":
                serviceAdressOptionsMock.Setup(m => m.Value)
                    .Returns(new ServiceAddressOptions()
                    {
                        OrdersService = "http://localhost:7042",
                        UserManagementService = "http://localhost:5002"
                    });
                break;
            default:
                serviceAdressOptionsMock.Setup(m => m.Value)
                    .Returns(new ServiceAddressOptions()
                    {
                        OrdersService = "http://localhost:7042",
                        UserManagementService = "http://localhost:7207"
                    });
                break;
        }

        _systemUnderTests = new OrdersClient(new HttpClient(), serviceAdressOptionsMock.Object);
        _loginClient = new LoginClient(new HttpClient(), serviceAdressOptionsMock.Object);

        var identityOptions = new IdentityServerApiOptions()
        {
            ClientId = "test.client",
            ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A"
        };

        var token = await _loginClient.GetApiTokenByClientSeceret(identityOptions);
        _systemUnderTests.HttpClient.SetBearerToken(token.AccessToken);
    }
    
    [TearDown]
    public void TearDown()
    {
        _loginClient?.Dispose();
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_order_THEN_it_is_being_added_to_database()
    {
        var expected = _fixture.Build<Order>()
            .With(o => o.Articles, _fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var addResponse = await _systemUnderTests.Add(expected);
        Assert.That(addResponse.IsSuccessfull);

        var getOneResponse = await _systemUnderTests.GetOne(addResponse.Payload);
        Assert.That(getOneResponse.IsSuccessfull);
        var actual = getOneResponse.Payload;

        AssertObjectsAreEqual(expected, actual);

        var removeResponse = await _systemUnderTests.Remove(addResponse.Payload);
        Assert.That(removeResponse.IsSuccessfull);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_add_several_orders_THEN_it_is_being_added_to_database()
    {
        var expected1 = _fixture.Build<Order>()
            .With(o => o.Articles, _fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var expected2 = _fixture.Build<Order>()
            .With(o => o.Articles, _fixture.CreateMany<OrderedArticle>().ToList())
            .Create();

        var ordersToAdd = new[] { expected1, expected2 };

        var addRangeResponse = await _systemUnderTests.AddRange(ordersToAdd);
        Assert.That(addRangeResponse.IsSuccessfull);

        var getAllResponse = await _systemUnderTests.GetAll();
        Assert.That(getAllResponse.IsSuccessfull);

        var addedOrders = getAllResponse.Payload;

        foreach (var orderId in addRangeResponse.Payload)
        {
            var expectedOrder = ordersToAdd.Single(o => o.Id == orderId);
            var actualOrder = addedOrders.Single(o => o.Id == orderId);
            AssertObjectsAreEqual(expectedOrder, actualOrder);
        }

        var removeRangeResponse = await _systemUnderTests.RemoveRange(addRangeResponse.Payload);
        Assert.That(removeRangeResponse.IsSuccessfull);
    }

    [Test]
    public async Task GIVEN_Orders_Repo_Client_WHEN_I_update_order_THEN_it_is_being_update_in_database()
    {
        var orderedArticles = _fixture.CreateMany<OrderedArticle>().ToList();

        var expected = _fixture.Build<Order>()
            .With(o => o.Articles, orderedArticles)
            .Create();

        var addResponse = await _systemUnderTests.Add(expected);
        Assert.That(addResponse.IsSuccessfull);

        orderedArticles.ForEach(oa => oa.Name = _fixture.Create<string>());

        expected.UserId = _fixture.Create<Guid>();
        expected.AddressId = _fixture.Create<Guid>();
        expected.Articles = orderedArticles;

        var updateResponse = await _systemUnderTests.Update(expected);
        Assert.That(updateResponse.IsSuccessfull);
        var actual = updateResponse.Payload;

        AssertObjectsAreEqual(expected, actual);

        var removeResponse = await _systemUnderTests.Remove(addResponse.Payload);
        Assert.That(removeResponse.IsSuccessfull);
    }

    private void AssertObjectsAreEqual(Order expected, Order actual)
    {
        Assert.That(expected.Id, Is.EqualTo(actual.Id));
        Assert.That(expected.AddressId, Is.EqualTo(actual.AddressId));
        Assert.That(expected.UserId, Is.EqualTo(actual.UserId));
        Assert.That(expected.Created, Is.EqualTo(actual.Created));
        Assert.That(expected.Articles.Count, Is.EqualTo(actual.Articles.Count));
    }
}