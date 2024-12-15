using System;
using System.Reflection;
using OnlineShop.IdentityServer.Data;
using OnlineShop.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var identityConnectionString = Configuration.GetConnectionString("IdentityServerConnection");
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UsersConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

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
                            Configuration.GetConnectionString(identityConnectionString!),
                            sql => { 
                                sql.MigrationsAssembly(migrationAssembly);
                                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                            })
                        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(
                            Configuration.GetConnectionString(identityConnectionString!),
                            sql => {
                                sql.MigrationsAssembly(migrationAssembly);
                                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                            })
                        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
                })
                .AddAspNetIdentity<ApplicationUser>();

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