using BookAPI.DTOs;
using BookAPI.Entity;
using BookAPI.Parameters;
using BookAPI.Responses;
using BookAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
	/// <summary>
	/// Manages author resources, including retrieval, creation, updating, and deletion operations.
	/// </summary>
	/// <param name="authorService">The service layer handling author-related business logic.</param>
	[Route("api/authors")]
	[ApiController]
	public class AuthorsController(IAuthorService authorService)
		: ControllerBase
	{
		/// <summary>
		/// Retrieves a paginated list of authors with support for filtering and sorting.
		/// </summary>
		/// <param name="queryParameters">Query parameters containing pagination options, filters, and sorting choices.</param>
		/// <returns>A paginated list of author records wrapped in a standard API response.</returns>
		/// <response code="200">Authors retrieved successfully.</response>
		/// <response code="400">If input parameters fail validation constraints.</response>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AuthorDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetAuthors([FromQuery] AuthorQueryParameters queryParameters)
		{
			var authors = await authorService.GetAuthorsAsync(queryParameters);

			return Ok(ApiResponse<PaginatedResponse<AuthorDto>>.Ok(authors, "Authors retrieved successfully."));
		}

		/// <summary>
		/// Searches for authors matching a specific keyword with pagination.
		/// </summary>
		/// <param name="searchTerm">The string term used to match author names or bios.</param>
		/// <param name="pageNumber">The page index requested (defaults to 1).</param>
		/// <param name="pageSize">The number of records per page (defaults to 10).</param>
		/// <returns>A paginated list of matching author records wrapped in a standard API response.</returns>
		/// <response code="200">Search results retrieved successfully.</response>
		/// <response code="400">If page parameter values are invalid.</response>
		[HttpGet("search")]
		[AllowAnonymous]
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

		/// <summary>
		/// Retrieves details for a specific author by their unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author.</param>
		/// <returns>Author details wrapped in a standard API response.</returns>
		/// <response code="200">Author retrieved successfully.</response>
		/// <response code="404">If no author exists with the supplied ID.</response>
		[HttpGet("{id:guid}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetAuthorById(Guid id)
		{
			var author = await authorService.GetAuthorByIdAsync(id);

			return Ok(ApiResponse<AuthorDto>.Ok(author, "Author retrieved successfully."));
		}

		/// <summary>
		/// Creates a new author record.
		/// </summary>
		/// <param name="createAuthorDto">Data transfer object containing information for the new author.</param>
		/// <returns>The newly created author data wrapped in a standard API response.</returns>
		/// <response code="201">Author created successfully.</response>
		/// <response code="400">If validation checks on request body fail.</response>
		/// <response code="401">If the user is not authenticated.</response>
		/// <response code="403">If the user lacks the required Admin role.</response>
		/// <response code="409">If an author with the same unique attributes already exists.</response>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
		{
			var author = await authorService.CreateAuthorAsync(createAuthorDto);

			return CreatedAtAction(
				nameof(GetAuthorById),
				new { id = author.Id },
				ApiResponse<AuthorDto>.Ok(author, "Author created successfully."));
		}

		/// <summary>
		/// Updates an existing author by their unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author to update.</param>
		/// <param name="updateAuthorDto">Data transfer object containing updated author parameters.</param>
		/// <returns>The updated author details wrapped in a standard API response.</returns>
		/// <response code="200">Author updated successfully.</response>
		/// <response code="400">If request data fails validation checks.</response>
		/// <response code="401">If the user is not authenticated.</response>
		/// <response code="403">If the user lacks the required Admin role.</response>
		/// <response code="404">If the specified author ID does not exist.</response>
		/// <response code="409">If updating causes a conflict with another author record.</response>
		[HttpPut("{id:guid}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
		{
			var author = await authorService.UpdateAuthorAsync(id, updateAuthorDto);

			return Ok(ApiResponse<AuthorDto>.Ok(author, "Author updated successfully."));
		}

		/// <summary>
		/// Deletes an author from the system by their unique identifier.
		/// </summary>
		/// <param name="id">The unique identifier (GUID) of the author to delete.</param>
		/// <returns>A success response containing no payload data.</returns>
		/// <response code="200">Author deleted successfully.</response>
		/// <response code="401">If the user is not authenticated.</response>
		/// <response code="403">If the user lacks the required Admin role.</response>
		/// <response code="404">If the specified author ID does not exist.</response>
		[HttpDelete("{id:guid}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteAuthor(Guid id)
		{
			await authorService.DeleteAuthorAsync(id);

			return Ok(ApiResponse<object>.Ok(null!, "Author deleted successfully."));
		}
	}
}