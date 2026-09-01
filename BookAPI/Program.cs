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

// Service Registration Phase

/// <summary>
/// Registers API controllers for routing and handling incoming HTTP requests.
/// </summary>
builder.Services.AddControllers();

/// <summary>
/// Configures global exception handling infrastructure and standard RFC 7807 Problem Details.
/// </summary>
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

/// <summary>
/// Scans and registers all FluentValidation validators contained within the assembly.
/// </summary>
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

/// <summary>
/// Configures Entity Framework Core database context with SQL Server 
/// and registers ASP.NET Core Identity services utilizing Guid primary keys.
/// </summary>
builder.Services.AddDbContext<ApplicationDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequiredLength = 8;
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

/// <summary>
/// Configures JWT Bearer authentication scheme and token validation rules.
/// </summary>
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

/// <summary>
/// Configures AutoMapper profiles and registers application layer services into DI container.
/// </summary>
builder.Services.AddAutoMapper(cfg =>
{
	cfg.AddProfile<BookProfile>();
	cfg.AddProfile<AuthorProfile>();
});

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IAuthService, AuthService>();

/// <summary>
/// Configures OpenAPI 3.0 specification details and sets up global JWT Bearer security scheme for Scalar UI.
/// </summary>
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

// Database Seeding & Startup Operations

/// <summary>
/// Initializes and seeds required initial roles, administrator account, and seed data upon application startup.
/// </summary>
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<ApplicationDBContext>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
		var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

		// Seed "Admin" role if it does not exist
		if (!await roleManager.RoleExistsAsync("Admin"))
		{
			await roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Admin" });
		}

		// Seed initial System Admin user account
		var adminEmail = "admin@example.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);

		if (adminUser == null)
		{
			var newAdmin = new ApplicationUser
			{
				FullName = "System Admin",
				UserName = "admin",
				Email = adminEmail,
				EmailConfirmed = true
			};

			var result = await userManager.CreateAsync(newAdmin, "Password123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(newAdmin, "Admin");
			}
		}

		// Run domain entity database seeder
		await DatabaseSeeder.SeedAsync(context, roleManager);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database and roles.");
	}
}

// Middleware Pipeline Execution

/// <summary>
/// Configures global exception handling middleware.
/// </summary>
app.UseExceptionHandler();

/// <summary>
/// Configures OpenAPI endpoints and visual Interactive Documentation using Scalar UI in development mode.
/// </summary>
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

/// <summary>
/// Enables Authentication and Authorization middlewares.
/// </summary>
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Maps controller endpoints to the routing system.
/// </summary>
app.MapControllers();

/// <summary>
/// Runs the application and starts listening for HTTP requests.
/// </summary>
app.Run();