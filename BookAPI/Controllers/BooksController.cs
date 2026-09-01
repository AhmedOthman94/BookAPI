using BookAPI.DTOs;
using BookAPI.Entity;
using BookAPI.Parameters;
using BookAPI.Responses;
using BookAPI.Services;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
	/// <summary>
	/// Manages book catalog operations including retrieving, searching, creating, updating, and deleting books.
	/// </summary>
	/// <param name="bookService">The service layer handling book-related business logic.</param>
	[Route("api/books")]
	[ApiController]
	public class BooksController(IBookService bookService)
		: ControllerBase
	{
		/// <summary>
		/// Retrieves a paginated list of books filtered by search term, author, publication dates, and sorting parameters.
		/// </summary>
		/// <param name="queryParameters">Filtering, sorting, and pagination options for the book query.</param>
		/// <returns>A paginated list of book records wrapped in a standard API response.</returns>
		/// <response code="200">Books retrieved successfully.</response>
		/// <response code="400">If query parameters fail validation criteria.</response>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<BookDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetBooks([FromQuery] BookQueryParameters queryParameters)
		{
			var books = await bookService.GetBooksAsync(queryParameters);

			return Ok(ApiResponse<PaginatedResponse<BookDto>>.Ok(books, "Books retrieved successfully."));
		}

		/// <summary>
		/// Retrieves details for a specific book by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book.</param>
		/// <returns>The requested book details wrapped in a standard API response.</returns>
		/// <response code="200">Book retrieved successfully.</response>
		/// <response code="404">If no book exists with the specified ID.</response>
		[HttpGet("{id:guid}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetBookById(Guid id)
		{
			var book = await bookService.GetBookByIdAsync(id);

			return Ok(ApiResponse<BookDto>.Ok(book, "Book retrieved successfully."));
		}

		/// <summary>
		/// Creates a new book entry in the catalog.
		/// </summary>
		/// <param name="createBookDto">Data transfer object containing information for the new book.</param>
		/// <returns>The newly created book details wrapped in a standard API response.</returns>
		/// <response code="201">Book created successfully.</response>
		/// <response code="400">If input model parameters fail validation rules.</response>
		/// <response code="401">If the user is unauthenticated.</response>
		/// <response code="403">If the user lacks Admin permissions.</response>
		/// <response code="404">If the specified author ID does not exist.</response>
		/// <response code="409">If a book with the same title already exists.</response>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createBookDto)
		{
			var book = await bookService.CreateBookAsync(createBookDto);

			return CreatedAtAction(
				nameof(GetBookById),
				new { id = book.Id },
				ApiResponse<BookDto>.Ok(book, "Book created successfully."));
		}

		/// <summary>
		/// Updates an existing book entry by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book to update.</param>
		/// <param name="updateAuthorDto">Data transfer object containing updated book parameters.</param>
		/// <returns>The updated book details wrapped in a standard API response.</returns>
		/// <response code="200">Book updated successfully.</response>
		/// <response code="400">If request body fails validation constraints.</response>
		/// <response code="401">If the user is unauthenticated.</response>
		/// <response code="403">If the user lacks Admin permissions.</response>
		/// <response code="404">If the targeted book ID or foreign author ID is not found.</response>
		/// <response code="409">If updating creates a title collision with another book.</response>
		[HttpPut("{id:guid}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookDto updateAuthorDto)
		{
			var book = await bookService.UpdateBookAsync(id, updateAuthorDto);

			return Ok(ApiResponse<BookDto>.Ok(book, "Book updated successfully."));
		}

		/// <summary>
		/// Removes a book from the catalog by its unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the book to delete.</param>
		/// <returns>A success payload confirming deletion.</returns>
		/// <response code="200">Book deleted successfully.</response>
		/// <response code="401">If the user is unauthenticated.</response>
		/// <response code="403">If the user lacks Admin permissions.</response>
		/// <response code="404">If the specified book ID is not found.</response>
		[HttpDelete("{id:guid}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteBook(Guid id)
		{
			await bookService.DeleteBookAsync(id);

			return Ok(ApiResponse<object>.Ok(null!, "Book deleted successfully."));
		}
	}
}