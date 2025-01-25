using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.ApiService.Authorization;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.ArticlesService.Repos;
using OnlineShop.Library.Bearer;
using OnlineShop.Library.Clients;
using OnlineShop.Library.Clients.ArticlesService;
using OnlineShop.Library.Clients.IdentityServer;
using OnlineShop.Library.Clients.OrdersService;
using OnlineShop.Library.Clients.UserManagementService;
using OnlineShop.Library.Common.Interfaces;
using OnlineShop.Library.Constants;
using OnlineShop.Library.Data.Migrations;
using OnlineShop.Library.Options;
using OnlineShop.Library.OrdersService.Models;
using OnlineShop.Library.UserManagementService.Models;
using Scalar.AspNetCore;

namespace OnlineShop.ApiService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient<UsersClient>();
        builder.Services.AddHttpClient<RolesClient>();
        builder.Services.AddHttpClient<IdentityServerClient>();
        builder.Services.AddHttpClient<ArticlesClient>();
        builder.Services.AddHttpClient<PriceListsClient>();
        builder.Services.AddHttpClient<OrderedArticle>();
        builder.Services.AddHttpClient<Order>();
        
        builder.Services.AddTransient<ILoginClient, LoginClient>();
        builder.Services.AddTransient<IRolesClient, RolesClient>();
        builder.Services.AddTransient<IUsersClient, UsersClient>();
        builder.Services.AddTransient<IIdentityServerClient, IdentityServerClient>();
        builder.Services.AddTransient<IClientAuthorization, HttpClientAuthorization>();
        builder.Services.AddTransient<IRepoClient<Article>, ArticlesClient>();
        builder.Services.AddTransient<IRepoClient<PriceList>, PriceListsClient>();
        builder.Services.AddTransient<IRepoClient<OrderedArticle>, OrderedArticlesClient>();
        builder.Services.AddTransient<IRepoClient<Order>, OrdersClient>();
        
        builder.Services.Configure<IdentityServerApiOptions>(builder.Configuration.GetSection(IdentityServerApiOptions.SectionName));
        builder.Services.Configure<ServiceAddressOptions>(builder.Configuration.GetSection(ServiceAddressOptions.SectionName));

        builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        var serviceAddressOptions = new ServiceAddressOptions();
        builder.Configuration.GetSection(ServiceAddressOptions.SectionName).Bind(serviceAddressOptions);

        builder.Services.AddAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Authority = serviceAddressOptions.IdentityServer;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters() { ValidateAudience = false };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdConstants.ApiScope);
            });
        });

        /*builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder.WithOrigins("https://localhost:7163").AllowAnyHeader().AllowAnyMethod();
                policyBuilder.WithOrigins("http://localhost:5163").AllowAnyHeader().AllowAnyMethod();
            });
        });*/
        
        builder.Services.AddControllers();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireAuthorization("ApiScope");
        });
        
        app.Run();
    }
}