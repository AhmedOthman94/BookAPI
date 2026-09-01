using BookAPI.Entity;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data
{
	public static class DatabaseSeeder
	{
		public static async Task SeedAsync(ApplicationDBContext context)
		{
			if (context.Database.IsRelational())
			{
				await context.Database.MigrateAsync();
			}

			if (await context.Authors.AnyAsync() || await context.Books.AnyAsync())
			{
				return;
			}

			var authors = GenerateAuthors();
			await context.Authors.AddRangeAsync(authors);
			await context.SaveChangesAsync();

			var books = GenerateBooks(authors);
			await context.Books.AddRangeAsync(books);
			await context.SaveChangesAsync();
		}

		private static List<Author> GenerateAuthors()
		{
			return new List<Author>
			{
				new Author { Id = Guid.NewGuid(), Name = "Robert C. Martin", Bio = "Author of Clean Code and Clean Architecture.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Andrew Hunt", Bio = "Co-author of The Pragmatic Programmer.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "David Thomas", Bio = "Co-author of The Pragmatic Programmer.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Martin Fowler", Bio = "Expert in software architecture and refactoring.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Eric Evans", Bio = "Thought leader in Domain-Driven Design.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Jon Skeet", Bio = "Author of C# in Depth and top Stack Overflow contributor.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Stephen Cleary", Bio = "Expert in Async/Await and Concurrency in .NET.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Joseph Albahari", Bio = "Author of C# NutShell series.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Vaughn Vernon", Bio = "Author of Implementing Domain-Driven Design.", CreatedAt = DateTime.UtcNow },
				new Author { Id = Guid.NewGuid(), Name = "Steve McConnell", Bio = "Author of Code Complete.", CreatedAt = DateTime.UtcNow }
			};
		}

		private static List<Book> GenerateBooks(List<Author> authors)
		{
			var books = new List<Book>();
			var random = new Random(42);

			string[] topics = { "C#", "ASP.NET Core", "Entity Framework", "Clean Architecture", "Microservices", "Design Patterns", "SQL Server", "Docker", "Kubernetes", "Azure", "DevOps", "Unit Testing", "Domain-Driven Design", "GraphQL", "REST APIs" };
			string[] levels = { "Fundamentals", "Advanced", "Mastery", "Pro Guide", "In Action", "Deep Dive", "Best Practices", "Patterns and Practices" };

			for (int i = 1; i <= 120; i++)
			{
				var author = authors[random.Next(authors.Count)];
				var topic = topics[random.Next(topics.Length)];
				var level = levels[random.Next(levels.Length)];

				books.Add(new Book
				{
					Id = Guid.NewGuid(),
					Title = $"{topic} {level} - Vol. {i}",
					ISBN = $"978-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(10, 99)}-{i % 10}",
					Description = $"A comprehensive guide covering {topic} focusing on modern software engineering techniques.",
					Price = Math.Round((decimal)(random.NextDouble() * 80 + 20), 2),
					Stock = random.Next(5, 150),
					PublishedAt = DateTime.UtcNow.AddDays(-random.Next(30, 1800)),
					AuthorId = author.Id,
					CraetedAt = DateTime.UtcNow
				});
			}

			return books;
		}
	}
}
