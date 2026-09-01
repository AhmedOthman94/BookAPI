using BookAPI.DTOs;
using BookAPI.Parameters;
using BookAPI.Responses;

namespace BookAPI.Services.IServices
{
	public interface IBookService
	{
		Task<PaginatedResponse<BookDto>> GetBooksAsync(BookQueryParameters queryParameters); 

		Task<PaginatedResponse<BookDto>> SearchBooksAsync(string searchTerm,
							int pageNumber, int pageSize);

		Task<BookDto> GetBookByIdAsync(Guid id);

		Task<BookDto> CreateBookAsync(CreateBookDto createBookDto); 

		Task<BookDto> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto); 

		Task DeleteBookAsync(Guid id);
	}
}
