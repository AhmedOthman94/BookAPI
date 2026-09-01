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
	/// <summary>
	/// Service implementation for managing Author entities, including querying, pagination, searching, and CRUD operations.
	/// </summary>
	/// <param name="context">The database context instance for accessing author entity sets.</param>
	/// <param name="mapper">The AutoMapper instance for mapping entities and DTOs.</param>
	public class AuthorService(
		ApplicationDBContext context,
		IMapper mapper)
		: IAuthorService
	{
		/// <summary>
		/// Retrieves a paginated list of authors applying filtering, sorting, and projection to <see cref="AuthorDto"/>.
		/// </summary>

		/// <param name="queryParameters">Query parameters containing pagination, search term, and sorting preferences.</param>
		/// <returns>A <see cref="PaginatedResponse{T}"/> containing the paginated collection of <see cref="AuthorDto"/> items.</returns>
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

		/// <summary>
		/// Searches for authors matching a search keyword in their name or biography with pagination.
		/// </summary>

		/// <param name="searchTerm">The keyword used to filter authors by name or bio.</param>
		/// <param name="pageNumber">The requested page index (1-based).</param>
		/// <param name="pageSize">The maximum number of items per page.</param>
		/// <returns>A <see cref="PaginatedResponse{T}"/> containing matching <see cref="AuthorDto"/> items.</returns>
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

		/// <summary>
		/// Retrieves detailed information for a specific author by their unique identifier.
		/// </summary>

		/// <param name="id">The unique identifier (GUID) of the author.</param>
		/// <returns>The <see cref="AuthorDto"/> representing the author.</returns>
		/// <exception cref="AuthorNotFoundException">Thrown when no author is found with the specified ID.</exception>
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

		/// <summary>
		/// Creates a new author entry after validating that no duplicate name exists.
		/// </summary>

		/// <param name="createAuthorDto">The data transfer object containing new author details.</param>
		/// <returns>The created <see cref="AuthorDto"/> mapped from the saved entity.</returns>
		/// <exception cref="DuplicateAuthorException">Thrown when an author with the same name already exists.</exception>
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

		/// <summary>
		/// Updates an existing author's details while ensuring name uniqueness across other authors.
		/// </summary>

		/// <param name="id">The unique identifier (GUID) of the author to update.</param>
		/// <param name="updateAuthorDto">The data transfer object containing updated details.</param>
		/// <returns>The updated <see cref="AuthorDto"/> reflecting the changes.</returns>
		/// <exception cref="AuthorNotFoundException">Thrown when no author is found with the specified ID.</exception>
		/// <exception cref="DuplicateAuthorException">Thrown when another author already uses the provided name.</exception>
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

		/// <summary>
		/// Removes an author from the database by their unique identifier.
		/// </summary>

		/// <param name="id">The unique identifier (GUID) of the author to delete.</param>

		/// <exception cref="AuthorNotFoundException">Thrown when no author is found with the specified ID.</exception>
		public async Task DeleteAuthorAsync(Guid id)
		{
			var author = await context.Authors
				.FirstOrDefaultAsync(a => a.Id == id);

			if (author is null)
				throw new AuthorNotFoundException(id);

			context.Authors.Remove(author);

			await context.SaveChangesAsync();
		}

		/// <summary>
		/// Applies dynamic sorting to the author query based on specified query parameters.
		/// </summary>

		/// <param name="query">The input author queryable collection.</param>
		/// <param name="parameters">Query parameters containing sort key and direction preferences.</param>
		/// <returns>The sorted <see cref="IQueryable{Author}"/> query.</returns>
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