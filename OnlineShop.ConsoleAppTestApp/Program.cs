﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Library.Clients.IdentityServer;
using Microsoft.Extensions.Logging;
using OnlineShop.Library.Clients.UserManagementService;
using OnlineShop.Library.Options;

namespace OnlineShop.ConsoleAppTestApp;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<IdentityServerClient>();
                    services.AddHttpClient<UsersClient>();
                    services.AddHttpClient<RolesClient>();

                    services.AddTransient<AuthenticationServiceTest>();

                    var configurationBuilder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false);

                    IConfiguration configuration = configurationBuilder.Build();

                    services.Configure<IdentityServerApiOptions>(configuration.GetSection(IdentityServerApiOptions.SectionName));
                    services.Configure<ServiceAddressOptions>(configuration.GetSection(ServiceAddressOptions.SectionName));
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .UseConsoleLifetime();              

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var service = services.GetRequiredService<AuthenticationServiceTest>();

                    var rolesResult = await service.RunRolesClientTests(args);
                    var usersResult = await service.RunUsersClientTests(args);

                    Console.WriteLine(rolesResult);
                    Console.WriteLine(usersResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured: {ex.Message}");
                }
            }

            Console.ReadKey();

            return 0;
    }
}