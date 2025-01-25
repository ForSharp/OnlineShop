using AutoFixture;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Moq;
using OnlineShop.Library.Clients.UserManagementService;
using OnlineShop.Library.Common.Models;
using OnlineShop.Library.Options;

namespace OnlineShop.ApiService.ApiTests;

public class BaseRepoControllerTests
{
    protected readonly Fixture Fixture = new Fixture();
    protected ILoginClient LoginClient;
    protected HttpClient SystemUnderTests;

    public BaseRepoControllerTests()
    {
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [SetUp]
    public async Task Setup()
    {
        var serviceAdressOptionsMock = new Mock<IOptions<ServiceAddressOptions>>();

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        switch (env)
        {
            case "Docker":
                serviceAdressOptionsMock.Setup(m => m.Value).Returns(new ServiceAddressOptions()
                {
                    OrdersService = "http://localhost:7042",
                    //IdentityServer = "https://localhost:5001",
                    UserManagementService = "http://localhost:7207",
                    ApiService = "http://localhost:7251"
                });
                break;
            default:
                serviceAdressOptionsMock.Setup(m => m.Value).Returns(new ServiceAddressOptions()
                {
                    OrdersService = "https://localhost:7042",
                    IdentityServer = "https://localhost:5001",
                    UserManagementService = "https://localhost:7207",
                    ApiService = "https://localhost:7251"
                });
                break;
        }
        SystemUnderTests = new HttpClient() { BaseAddress = new Uri(serviceAdressOptionsMock.Object.Value.ApiService) };
        LoginClient = new LoginClient(new HttpClient(), serviceAdressOptionsMock.Object);

        var token = await LoginClient.GetApiTokenByUsernameAndPassword(new IdentityServerUserNamePassword()
        {
            UserName = "nikita",
            Password = "Pass_123"
        });

        SystemUnderTests.SetBearerToken(token.AccessToken);
    }
    
    [TearDown]
    public void TearDown()
    {
        LoginClient?.Dispose();
        SystemUnderTests.Dispose();
    }
}