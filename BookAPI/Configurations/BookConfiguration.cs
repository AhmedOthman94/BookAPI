using BookAPI.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookAPI.Configurations
{
	public class BookConfiguration : IEntityTypeConfiguration<Book>
	{
		public void Configure(EntityTypeBuilder<Book> builder)
		{
			builder.ToTable("Books");

			builder.HasKey(b => b.Id);

			builder.Property(b => b.Id)
				.ValueGeneratedOnAdd();

			builder.Property(b => b.Title)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(b => b.ISBN)
				.IsRequired()
				.HasMaxLength(17); 

			builder.HasIndex(b => b.ISBN)
				.IsUnique();

			builder.Property(b => b.Description)
				.HasMaxLength(2000)
				.IsRequired(false);

			builder.Property(b => b.Price)
				.HasPrecision(18, 2) 
				.IsRequired();

			builder.Property(b => b.Stock)
				.IsRequired()
				.HasDefaultValue(0);

			builder.Property(b => b.PublishedAt)
				.IsRequired();

			builder.Property(b => b.CreatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

			builder.Property(b => b.UpdateAt)
				.IsRequired(false);

			builder.HasOne(b => b.Author)
				.WithMany(a => a.Books)
				.HasForeignKey(b => b.AuthorId)
				.OnDelete(DeleteBehavior.Restrict); 
		}
	}
}
