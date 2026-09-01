using BookAPI.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookAPI.Configurations
{
	public class AuthorConfiguration : IEntityTypeConfiguration<Author>
	{
		public void Configure(EntityTypeBuilder<Author> builder) 
		{
			builder.ToTable("Authors");

			builder.HasKey(a => a.Id);

			builder.Property(a => a.Id)
				.ValueGeneratedOnAdd();

			builder.Property(a => a.Name)
				.IsRequired()
				.HasMaxLength(150);

			builder.Property(a => a.Bio)
				.HasMaxLength(1000)
				.IsRequired(false);

			builder.Property(a => a.CreatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

			builder.HasMany(a => a.Books)
				.WithOne(b => b.Author)
				.HasForeignKey(b => b.AuthorId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
