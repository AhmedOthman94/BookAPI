using BookAPI.DTOs;
using BookAPI.Entity;
using BookAPI.Parameters;
using BookAPI.Responses;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
	[Route("api/authors")]
	[ApiController]
	public class AuthorsController (IAuthorService authorService)
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AuthorDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetAuthors([FromQuery] AuthorQueryParameters queryParameters)
		{
			var authors = await authorService.GetAuthorsAsync(queryParameters);

			return Ok(ApiResponse<PaginatedResponse<AuthorDto>>.Ok(authors, "Authors retrieved successfully."));
		}

		[HttpGet("search")]
		[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AuthorDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> SearchAuthors(
			[FromQuery] string searchTerm,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10)
		{
			var authors = await authorService.SearchAuthorsAsync(searchTerm, pageNumber, pageSize);

			return Ok(ApiResponse<PaginatedResponse<AuthorDto>>.Ok(authors, "Search results retrieved successfully."));
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetAuthorById(Guid id)
		{
			var author = await authorService.GetAuthorByIdAsync(id);

			return Ok(ApiResponse<AuthorDto>.Ok(author, "Author retrieved successfully."));
		}

		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
		{
			var author = await authorService.CreateAuthorAsync(createAuthorDto);

			return CreatedAtAction(
				nameof(GetAuthorById),
				new { id = author.Id },
				ApiResponse<AuthorDto>.Ok(author, "Author created successfully."));
		}

		[HttpPut("{id:guid}")]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
		{
			var author = await authorService.UpdateAuthorAsync(id, updateAuthorDto);

			return Ok(ApiResponse<AuthorDto>.Ok(author, "Author updated successfully."));
		}

		[HttpDelete("{id:guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteAuthor(Guid id)
		{
			await authorService.DeleteAuthorAsync(id);

			return Ok(ApiResponse<object>.Ok(null!, "Author deleted successfully."));
		}
	}
}
