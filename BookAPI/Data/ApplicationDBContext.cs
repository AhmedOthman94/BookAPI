using BookAPI.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data
{
	public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
	: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
	{
		public DbSet<Author> Authors { get; set; }
		public DbSet<Book> Books { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);
		}


	}
}
