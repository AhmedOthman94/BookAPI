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
	public class AuthorService (ApplicationDBContext context,
								IMapper mapper)
	: IAuthorService
	{
		public async Task<PaginatedResponse<AuthorDto>> GetAuthorsAsync(
		AuthorQueryParameters queryParameters)
		{
			var query = context.Authors.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
			{
				var searchTerm = queryParameters.SearchTerm.Trim();

				query = query.Where(a =>
					a.Name.Contains(searchTerm) ||
					(a.Bio != null && a.Bio.Contains(searchTerm)));
			}

			query = ApplySorting(query, queryParameters);

			var totalCount = await query.CountAsync();

			var totalPages = (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize);

			var items = await query
				.Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
				.Take(queryParameters.PageSize)
				.ProjectTo<AuthorDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			return new PaginatedResponse<AuthorDto>
			{
				Items = items,
				PageNumber = queryParameters.PageNumber,
				PageSize = queryParameters.PageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}

		public async Task<PaginatedResponse<AuthorDto>> SearchAuthorsAsync(
			string searchTerm,
			int pageNumber,
			int pageSize)
		{
			var query = context.Authors.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				var term = searchTerm.Trim();
				query = query.Where(a =>
					a.Name.Contains(term) ||
					(a.Bio != null && a.Bio.Contains(term)));
			}

			var totalCount = await query.CountAsync();

			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			var items = await query
				.OrderBy(a => a.Name)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ProjectTo<AuthorDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			return new PaginatedResponse<AuthorDto>
			{
				Items = items,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}

		public async Task<AuthorDto> GetAuthorByIdAsync(Guid id)
		{
			var authorDto = await context.Authors
				.AsNoTracking()
				.Where(a => a.Id == id)
				.ProjectTo<AuthorDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();

			if (authorDto is null)
				throw new AuthorNotFoundException(id);

			return authorDto;
		}

		public async Task<AuthorDto> CreateAuthorAsync(
			CreateAuthorDto createAuthorDto)
		{
			var duplicateAuthor = await context.Authors
				.AnyAsync(a => a.Name == createAuthorDto.Name);

			if (duplicateAuthor)
				throw new DuplicateAuthorException(createAuthorDto.Name);

			var author = mapper.Map<Author>(createAuthorDto);

			author.Id = Guid.NewGuid();
			author.CreatedAt = DateTime.UtcNow;

			await context.Authors.AddAsync(author);
			await context.SaveChangesAsync();

			return mapper.Map<AuthorDto>(author);
		}

		public async Task<AuthorDto> UpdateAuthorAsync(
			Guid id,
			UpdateAuthorDto updateAuthorDto)
		{
			var author = await context.Authors
				.FirstOrDefaultAsync(a => a.Id == id);

			if (author is null)
				throw new AuthorNotFoundException(id);

			var duplicateAuthor = await context.Authors
				.AnyAsync(a =>
					a.Id != id &&
					a.Name == updateAuthorDto.Name);

			if (duplicateAuthor)
				throw new DuplicateAuthorException(updateAuthorDto.Name);

			mapper.Map(updateAuthorDto, author);

			await context.SaveChangesAsync();

			return mapper.Map<AuthorDto>(author);
		}

		public async Task DeleteAuthorAsync(Guid id)
		{
			var author = await context.Authors
				.FirstOrDefaultAsync(a => a.Id == id);

			if (author is null)
				throw new AuthorNotFoundException(id);

			context.Authors.Remove(author);

			await context.SaveChangesAsync();
		}

		private static IQueryable<Author> ApplySorting(
			IQueryable<Author> query,
			AuthorQueryParameters parameters)
		{
			var sortBy = parameters.SortBy?.Trim().ToLower();

			return sortBy switch
			{
				"name" => parameters.SortDescending
					? query.OrderByDescending(a => a.Name)
					: query.OrderBy(a => a.Name),

				"createdat" => parameters.SortDescending
					? query.OrderByDescending(a => a.CreatedAt)
					: query.OrderBy(a => a.CreatedAt),

				_ => query.OrderBy(a => a.Name)
			};
		}
	}
}
