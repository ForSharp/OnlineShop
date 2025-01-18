using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.Library.ArticlesService.Models;
using OnlineShop.Library.ArticlesService.Repos;
using OnlineShop.Library.Bearer;
using OnlineShop.Library.Common.Interfaces;
using OnlineShop.Library.Constants;
using OnlineShop.Library.Data.Migrations;
using OnlineShop.Library.Options;
using OnlineShop.Library.UserManagementService.Models;
using Scalar.AspNetCore;

namespace OnlineShop.ArticlesService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionNames.OrdersConnection)));
        builder.Services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionNames.UsersConnection)));
        
        builder.Services.AddTransient<IRepo<Article>, ArticlesRepo>();
        builder.Services.AddTransient<IRepo<PriceList>, PriceListsRepo>();
        
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

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