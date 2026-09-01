using BookAPI.DTOs;
using BookAPI.Parameters;
using BookAPI.Responses;

namespace BookAPI.Services.IServices
{
	/// <summary>
	/// Service interface for managing Book domain operations, business logic, and querying.
	/// </summary>
	public interface IBookService
	{
		/// <summary>
		/// Retrieves a paginated list of books based on filtering, sorting, and pagination query parameters.
		/// </summary>
		/// <param name="queryParameters">Query parameters including filtering criteria, sorting options, and pagination controls.</param>
		/// <returns>A paginated response containing a collection of <see cref="BookDto"/> items.</returns>
		Task<PaginatedResponse<BookDto>> GetBooksAsync(BookQueryParameters queryParameters);

		/// <summary>
		/// Searches books matching a specified search keyword with pagination support.
		/// </summary>
		/// <param name="searchTerm">The search term to match against book titles, descriptions, or authors.</param>
		/// <param name="pageNumber">The requested page number.</param>
		/// <param name="pageSize">The maximum number of items to return per page.</param>
		/// <returns>A paginated response containing matching <see cref="BookDto"/> items.</returns>
		Task<PaginatedResponse<BookDto>> SearchBooksAsync(string searchTerm, int pageNumber, int pageSize);

		/// <summary>
		/// Retrieves a single book by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book.</param>
		/// <returns>The <see cref="BookDto"/> representing the requested book.</returns>
		Task<BookDto> GetBookByIdAsync(Guid id);

		/// <summary>
		/// Creates a new book record in the system.
		/// </summary>
		/// <param name="createBookDto">The data transfer object containing the details for creating a book.</param>
		/// <returns>The newly created <see cref="BookDto"/> object.</returns>
		Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);

		/// <summary>
		/// Updates an existing book's details.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book to update.</param>
		/// <param name="updateBookDto">The data transfer object containing updated book details.</param>
		/// <returns>The updated <see cref="BookDto"/> object.</returns>
		Task<BookDto> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto);

		/// <summary>
		/// Deleteing an existing book.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book to update.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeleteBookAsync(Guid id);
	}
}