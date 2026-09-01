using BookAPI.DTOs;
using BookAPI.Parameters;
using BookAPI.Responses;

namespace BookAPI.Services.IServices
{
	/// <summary>
	/// Service interface for managing Author domain operations and business logic.
	/// </summary>
	public interface IAuthorService
	{
		/// <summary>
		/// Retrieves a paginated list of authors based on filtering, sorting, and pagination parameters.
		/// </summary>
		/// <param name="queryParameters">Query parameters including page number, page size, and sorting criteria.</param>
		/// <returns>A paginated response containing a collection of <see cref="AuthorDto"/> items.</returns>
		Task<PaginatedResponse<AuthorDto>> GetAuthorsAsync(AuthorQueryParameters queryParameters);

		/// <summary>
		/// Searches authors matching a specified search term with pagination support.
		/// </summary>
		/// <param name="searchTerm">The keyword to search within author names or details.</param>
		/// <param name="pageNumber">The requested page number.</param>
		/// <param name="pageSize">The number of items per page.</param>
		/// <returns>A paginated response containing matching <see cref="AuthorDto"/> items.</returns>
		Task<PaginatedResponse<AuthorDto>> SearchAuthorsAsync(string searchTerm, int pageNumber, int pageSize);

		/// <summary>
		/// Retrieves a single author by their unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author.</param>
		/// <returns>The <see cref="AuthorDto"/> representing the found author.</returns>
		Task<AuthorDto> GetAuthorByIdAsync(Guid id);

		/// <summary>
		/// Creates a new author entry in the system.
		/// </summary>
		/// <param name="createAuthorDto">The data transfer object containing author creation details.</param>
		/// <returns>The newly created <see cref="AuthorDto"/> object.</returns>
		Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);

		/// <summary>
		/// Updates an existing author's information.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author to update.</param>
		/// <param name="updateAuthorDto">The data transfer object containing updated author details.</param>
		/// <returns>The updated <see cref="AuthorDto"/> object.</returns>
		Task<AuthorDto> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateAuthorDto);

		/// <summary>
		/// Deletes an author from the system by their unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author to delete.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeleteAuthorAsync(Guid id);
	}
}