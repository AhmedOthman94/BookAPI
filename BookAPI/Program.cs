using System.Text;
using BookAPI.Data;
using BookAPI.Entity;
using BookAPI.Middleware;
using BookAPI.Profiles;
using BookAPI.Services;
using BookAPI.Services.IServices;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Core Services
builder.Services.AddControllers();

// Global Exception Handling & Problem Details
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// FluentValidation Registration
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Database & ASP.NET Core Identity Configuration
builder.Services.AddDbContext<ApplicationDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

// Authentication & JWT Setup
builder.Services.AddAuthentication(opts =>
{
	opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidateLifetime = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
		),
		ClockSkew = TimeSpan.Zero
	};
});

// AutoMapper & Application Services (Dependency Injection)
builder.Services.AddAutoMapper(cfg =>
{
	cfg.AddProfile<BookProfile>();
	cfg.AddProfile<AuthorProfile>();
});

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();

builder.Services.AddScoped<IAuthService, AuthService>();

// OpenAPI Document Configuration
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

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<ApplicationDBContext>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

		await DatabaseSeeder.SeedAsync(context, roleManager);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database and roles.");
	}
}

// Request Pipeline & Middlewares
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();

	app.MapScalarApiReference(options =>
	{
		options
			.WithTitle("Book Management System API")
			.WithTheme(ScalarTheme.Solarized)
			.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
			.AddPreferredSecuritySchemes("Bearer");
	});
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();