using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Library.Common.Models;
using OnlineShop.Library.UserManagementService.Models;

namespace OnlineShop.Library.Data.Migrations
{
    public class UsersDbContext : IdentityDbContext<ApplicationUser>
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Address>()
                .Property(a => a.Id)
                .HasDefaultValueSql("NEWID()");
            
            base.OnModelCreating(builder);
        }
    }
}