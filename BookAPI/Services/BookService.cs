using AutoMapper;
using AutoMapper.QueryableExtensions;
using BookAPI.Data;
using BookAPI.DTOs;
using BookAPI.Entity;
using BookAPI.Exceptions;
using BookAPI.Parameters;
using BookAPI.Responses;
using BookAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Services
{
	public class BookService (ApplicationDBContext context,
								IMapper mapper)
	: IBookService
	{
		public async Task<PaginatedResponse<BookDto>> GetBooksAsync(
		BookQueryParameters queryParameters)
		{
			var query = context.Books.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
			{
				var searchTerm = queryParameters.SearchTerm.Trim();

				query = query.Where(b =>
					b.Title.Contains(searchTerm) ||
					(b.Description != null && b.Description.Contains(searchTerm)) ||
					b.Author.Name.Contains(searchTerm));
			}

			if (queryParameters.AuthorId.HasValue)
			{
				query = query.Where(b => b.AuthorId == queryParameters.AuthorId.Value);
			}

			if (queryParameters.PublishedFrom.HasValue)
			{
				query = query.Where(b => b.PublishedAt >= queryParameters.PublishedFrom.Value);
			}

			if (queryParameters.PublishedTo.HasValue)
			{
				query = query.Where(b => b.PublishedAt <= queryParameters.PublishedTo.Value);
			}

			query = ApplySorting(query, queryParameters);

			var totalCount = await query.CountAsync();

			var totalPages = (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize);

			var items = await query
				.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
				.Take(queryParameters.PageSize)
				.ProjectTo<BookDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			return new PaginatedResponse<BookDto>
			{
				Items = items,
				PageNumber = queryParameters.PageNumber,
				PageSize = queryParameters.PageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}

		public async Task<PaginatedResponse<BookDto>> SearchBooksAsync(
			string searchTerm,
			int pageNumber,
			int pageSize)
		{
			var query = context.Books.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				var term = searchTerm.Trim();
				query = query.Where(b =>
					b.Title.Contains(term) ||
					(b.Description != null && b.Description.Contains(term)) ||
					b.Author.Name.Contains(term));
			}

			var totalCount = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			var items = await query
				.OrderBy(b => b.Title)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ProjectTo<BookDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			return new PaginatedResponse<BookDto>
			{
				Items = items,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}

		public async Task<BookDto> GetBookByIdAsync(Guid id)
		{
			var bookDto = await context.Books
				.AsNoTracking()
				.Where(b => b.Id == id)
				.ProjectTo<BookDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();

			if (bookDto is null)
				throw new BookNotFoundException(id);

			return bookDto;
		}

		public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
		{
			var authorExists = await context.Authors
				.AnyAsync(a => a.Id == createBookDto.AuthorId);

			if (!authorExists)
				throw new AuthorNotFoundException(createBookDto.AuthorId);

			var duplicateBook = await context.Books
				.AnyAsync(b => b.Title == createBookDto.Title);

			if (duplicateBook)
				throw new DuplicateBookException(createBookDto.Title);

			var book = mapper.Map<Book>(createBookDto);
			book.Id = Guid.NewGuid();
			book.CreatedAt = DateTime.UtcNow;

			await context.Books.AddAsync(book);
			await context.SaveChangesAsync();

			return await GetBookByIdAsync(book.Id);
		}

		public async Task<BookDto> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto)
		{
			var book = await context.Books.FirstOrDefaultAsync(b => b.Id == id);

			if (book is null)
				throw new BookNotFoundException(id);

			var authorExists = await context.Authors
				.AnyAsync(a => a.Id == updateBookDto.AuthorId);

			if (!authorExists)
				throw new AuthorNotFoundException(updateBookDto.AuthorId);

			var duplicateBook = await context.Books
				.AnyAsync(b => b.Id != id && b.Title == updateBookDto.Title);

			if (duplicateBook)
				throw new DuplicateBookException(updateBookDto.Title);

			mapper.Map(updateBookDto, book);
			book.UpdateAt = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return await GetBookByIdAsync(book.Id);
		}

		public async Task DeleteBookAsync(Guid id)
		{
			var book = await context.Books.FirstOrDefaultAsync(b => b.Id == id);

			if (book is null)
				throw new BookNotFoundException(id);

			context.Books.Remove(book);
			await context.SaveChangesAsync();
		}

		private static IQueryable<Book> ApplySorting(
			IQueryable<Book> query,
			BookQueryParameters parameters)
		{
			var sortBy = parameters.SortBy?.Trim().ToLower();

			return sortBy switch
			{
				"title" => parameters.SortDescending
					? query.OrderByDescending(b => b.Title)
					: query.OrderBy(b => b.Title),

				"price" => parameters.SortDescending
					? query.OrderByDescending(b => b.Price)
					: query.OrderBy(b => b.Price),

				"stock" => parameters.SortDescending
					? query.OrderByDescending(b => b.Stock)
					: query.OrderBy(b => b.Stock),

				"publishedat" => parameters.SortDescending
					? query.OrderByDescending(b => b.PublishedAt)
					: query.OrderBy(b => b.PublishedAt),

				"createdat" => parameters.SortDescending
					? query.OrderByDescending(b => b.CreatedAt)
					: query.OrderBy(b => b.CreatedAt),

				_ => query.OrderBy(b => b.Title)
			};
		}
	}
}
