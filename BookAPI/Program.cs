using System.Text;
using BookAPI.Data;
using BookAPI.Middleware;
using BookAPI.Profiles;
using BookAPI.Services;
using BookAPI.Services.IServices;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options =>
{
	options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
	options.AddDocumentTransformer((document, context, cancellationToken) =>
	{
		document.Info = new()
		{
			Title = "Book Management System API",
			Version = context.DocumentName,
			Description = "A robust and scalable RESTful API built with .NET 10 to manage Books and Authors using Clean Architecture principles."
		};

		document.Components ??= new OpenApiComponents();
		document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

		document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
		{
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			Description = "Input your JWT bearer token to access secured endpoints."
		});

		document.Security =
		[
			new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecuritySchemeReference("Bearer"),
						[]
					}
				}
		];

		return Task.CompletedTask;
	});
});

builder.Services.AddDbContext<ApplicationDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg =>
{
	cfg.AddProfile<BookProfile>();
	cfg.AddProfile<AuthorProfile>();
});


builder.Services.AddAuthentication(opts => 
{
	opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new()
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
		)
	};
});

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<ApplicationDBContext>();
		await DatabaseSeeder.SeedAsync(context);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

app.UseExceptionHandler();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();

	app.MapScalarApiReference(options =>
	{
		options
			.WithTitle("Book Management System API")
			.WithTheme(ScalarTheme.Purple)
			.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
	});
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
