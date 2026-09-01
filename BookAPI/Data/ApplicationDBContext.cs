using BookAPI.Entity;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data
{
	public class ApplicationDBContext (DbContextOptions<ApplicationDBContext> options)
	: DbContext(options)
	{
		public DbSet<Author> Authors { get; set; }
		public DbSet<Book> Books { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);
		}


	}
}
