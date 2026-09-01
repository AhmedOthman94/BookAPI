using BookAPI.DTOs;
using BookAPI.Parameters;
using BookAPI.Responses;

namespace BookAPI.Services.IServices
{
	public interface IAuthorService
	{
		Task<PaginatedResponse<AuthorDto>> GetAuthorsAsync(AuthorQueryParameters queryParameters);

		Task<PaginatedResponse<AuthorDto>> SearchAuthorsAsync(string searchTerm, 
						int pageNumber, int pageSize);

		Task<AuthorDto> GetAuthorByIdAsync(Guid id);

		Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);

		Task<AuthorDto> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateAuthorDto);

		Task DeleteAuthorAsync(Guid id);
	}
}
