using AutoFixture;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Moq;
using OnlineShop.Library.Clients;
using OnlineShop.Library.Clients.UserManagementService;
using OnlineShop.Library.Common.Interfaces;
using OnlineShop.Library.Options;

namespace OnlineShop.ArticlesService.ApiTests;

public abstract class ArticleServiceRepoBaseApiTest<TClient, TEntity> 
        where TClient : IRepoClient<TEntity>, IHttpClientContainer
        where TEntity : IIdentifiable
    {
        protected readonly Fixture Fixture = new Fixture();
        protected IOptions<ServiceAddressOptions> ServiceAdressOptions;
        protected IdentityServerApiOptions IdentityServerApiOptions;
        protected ILoginClient LoginClient;
        protected TClient SystemUnderTests;

        public ArticleServiceRepoBaseApiTest()
        {
            ConfigureFixture();
        }

        protected virtual void ConfigureFixture()
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [SetUp]
        public async Task Setup()
        {
            SetServiceAdressOptions();
            SetIdentityServerApiOptions();

            LoginClient = new LoginClient(new HttpClient(), ServiceAdressOptions);

            CreateSystemUnderTests(); 
            await AuthorizeSystemUnderTests();
        }
        
        [TearDown]
        public void TearDown()
        {
            LoginClient?.Dispose();
        }

        protected virtual void SetServiceAdressOptions()
        {
            var serviceAdressOptionsMock = new Mock<IOptions<ServiceAddressOptions>>();

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            switch (env)
            {
                case "Docker":
                    serviceAdressOptionsMock.Setup(m => m.Value)
                        .Returns(new ServiceAddressOptions() { 
                            ArticlesService = "http://localhost:7042", 
                            UserManagementService = "http://localhost:7207"
                        });
                    break;
                default:
                    serviceAdressOptionsMock.Setup(m => m.Value)
                        .Returns(new ServiceAddressOptions() { 
                            ArticlesService = "https://localhost:7042", 
                            UserManagementService = "https://localhost:7207"
                        });
                    break;
            }
            
            ServiceAdressOptions = serviceAdressOptionsMock.Object;
        }

        protected virtual void SetIdentityServerApiOptions()
        {
            IdentityServerApiOptions = new IdentityServerApiOptions()
            {
                ClientId = "test.client",
                ClientSecret = "511536EF-F270-4058-80CA-1C89C192F69A"
            };
        }

        protected virtual async Task AuthorizeSystemUnderTests()
        {
            var token = await LoginClient.GetApiTokenByClientSeceret(IdentityServerApiOptions);
            SystemUnderTests.HttpClient.SetBearerToken(token.AccessToken);
        }

        /// <summary>
        /// Creates an instance of type TClient.
        /// </summary>
        protected abstract void  CreateSystemUnderTests();

        [Test]
        public async virtual Task GIVEN_Repo_Client_WHEN_I_add_entity_THEN_it_is_being_added_to_database()
        {
            var expected = CreateExpectedEntity();

            var addResponse = await SystemUnderTests.Add(expected);
            Assert.That(addResponse.IsSuccessfull);

            var getOneResponse = await SystemUnderTests.GetOne(addResponse.Payload);
            Assert.That(getOneResponse.IsSuccessfull);
            var actual = getOneResponse.Payload;

            AssertObjectsAreEqual(expected, actual);

            var removeResponse = await SystemUnderTests.Remove(addResponse.Payload);
            Assert.That(removeResponse.IsSuccessfull);
        }

        [Test]
        public async virtual Task GIVEN_Repo_Client_WHEN_I_add_several_entities_THEN_it_is_being_added_to_database()
        {
            var expected1 = CreateExpectedEntity();
            var expected2 = CreateExpectedEntity();

            var entitiesToAdd = new[] { expected1, expected2 };

            var addRangeResponse = await SystemUnderTests.AddRange(entitiesToAdd);
            Assert.That(addRangeResponse.IsSuccessfull);

            var getAllResponse = await SystemUnderTests.GetAll();
            Assert.That(getAllResponse.IsSuccessfull);

            var addedEntities = getAllResponse.Payload;

            foreach (var entityId in addRangeResponse.Payload)
            {
                var expectedEntity = entitiesToAdd.Single(o => o.Id == entityId);
                var actualEntity = addedEntities.Single(o => o.Id == entityId);
                AssertObjectsAreEqual(expectedEntity, actualEntity);
            }

            var removeRangeResponse = await SystemUnderTests.RemoveRange(addRangeResponse.Payload);
            Assert.That(removeRangeResponse.IsSuccessfull);
        }

        [Test]
        public async virtual Task GIVEN_Repo_Client_WHEN_I_update_entuty_THEN_it_is_being_updated_in_database()
        {
            var expected = CreateExpectedEntity();

            var addResponse = await SystemUnderTests.Add(expected);
            Assert.That(addResponse.IsSuccessfull);

            AmendExpectedEntityForUpdating(expected);

            var updateResponse = await SystemUnderTests.Update(expected);
            Assert.That(updateResponse.IsSuccessfull);
            var actual = updateResponse.Payload;

            AssertObjectsAreEqual(expected, actual);

            var removeResponse = await SystemUnderTests.Remove(addResponse.Payload);
            Assert.That(removeResponse.IsSuccessfull);
        }

        protected abstract void AssertObjectsAreEqual(TEntity expected, TEntity actual);

        protected virtual TEntity CreateExpectedEntity() => Fixture.Build<TEntity>().Create();

        protected abstract void AmendExpectedEntityForUpdating(TEntity expected);
    }