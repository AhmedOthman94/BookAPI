namespace BookAPI.Entity
{
	public class ApiResponse<T>
	{
		public int StatusCode { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public T? Data { get; set; }
		public List<string> Errors { get; set; } = new();
		public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

		// Base factory method
		public static ApiResponse<T> Create(int statusCode, bool success, string message, T? data, List<string>? errors = null)
		{
			return new ApiResponse<T>
			{
				StatusCode = statusCode,
				Success = success,
				Message = message,
				Data = data,
				Errors = errors ?? new List<string>(),
				TimeStamp = DateTime.UtcNow
			};
		}

		// 200 OK
		public static ApiResponse<T> Ok(T? data, string message = "Request completed successfully.")
			=> Create(200, true, message, data);

		// 201 Created
		public static ApiResponse<T> CreatedAt(T? data, string message = "Resource created successfully.")
			=> Create(201, true, message, data);

		// 204 No Content
		public static ApiResponse<T> NoContent(string message = "No content available.")
			=> Create(204, true, message, default);

		// 400 Bad Request
		public static ApiResponse<T> BadRequest(string message = "Invalid request payload.", List<string>? errors = null)
			=> Create(400, false, message, default, errors);

		// 404 Not Found
		public static ApiResponse<T> NotFound(string message = "Requested resource was not found.")
			=> Create(404, false, message, default);

		// 409 Conflict
		public static ApiResponse<T> Conflict(string message = "A conflict occurred with the current state of the resource.", List<string>? errors = null)
			=> Create(409, false, message, default, errors);

		// 500 Internal Server Error
		public static ApiResponse<T> InternalServerError(string message = "An unexpected error occurred on the server.", List<string>? errors = null)
			=> Create(500, false, message, default, errors);
	}
}