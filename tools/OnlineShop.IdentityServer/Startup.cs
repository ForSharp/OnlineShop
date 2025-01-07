using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineShop.Library.Constants;
using OnlineShop.Library.Data.Migrations;
using OnlineShop.Library.UserManagementService.Models;

namespace OnlineShop.IdentityServer
{
    public class Startup(IWebHostEnvironment environment, IConfiguration configuration)
    {
        private IWebHostEnvironment Environment { get; } = environment;
        private IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var identityConnectionString = Configuration.GetConnectionString(ConnectionNames.IdentityServerConnection);
            
            services.AddDbContext<UsersDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(ConnectionNames.UsersConnection)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<UsersDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddDebug();
            });

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                })
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(
                            identityConnectionString,
                            sql => { 
                                sql.MigrationsAssembly(migrationAssembly);
                                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                            })
                        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(
                            identityConnectionString,
                            sql => {
                                sql.MigrationsAssembly(migrationAssembly);
                                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                            })
                        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryApiScopes(Config.ApiScopes);

            builder.AddDeveloperSigningCredential();
            services.AddAuthentication();
        }

        [Obsolete("Obsolete")]
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}