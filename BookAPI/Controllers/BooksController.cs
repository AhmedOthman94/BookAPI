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
	[Route("api/books")]
	[ApiController]
	public class BooksController(IBookService bookService)
		: ControllerBase
	{
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<BookDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetBooks([FromQuery] BookQueryParameters queryParameters)
		{
			var books = await bookService.GetBooksAsync(queryParameters);

			return Ok(ApiResponse<PaginatedResponse<BookDto>>.Ok(books, "Books retrieved successfully."));
		}

		[HttpGet("{id:guid}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetBookById(Guid id)
		{
			var book = await bookService.GetBookByIdAsync(id);

			return Ok(ApiResponse<BookDto>.Ok(book, "Book retrieved successfully."));
		}

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