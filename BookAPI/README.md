# BookAPI

A production-oriented **RESTful Web API** built with **ASP.NET Core .NET 10**, designed for managing books, authors, users, authentication, authorization, and refresh-token-based security.

The project demonstrates modern **Clean Architecture principles**, **ASP.NET Core Identity**, **JWT Authentication**, **Entity Framework Core**, **AutoMapper**, **FluentValidation**, centralized exception handling, pagination, filtering, searching, sorting, and database seeding.

---

## 🚀 Features

### 📚 Book Management

* Create, read, update, and delete books.
* Associate books with authors.
* Search books by:

  * Title
  * Description
  * Author name
* Filter books by:

  * Author
  * Publication date range
* Sort books by:

  * Title
  * Price
  * Stock
  * Published date
  * Created date
* Pagination with metadata.
* Duplicate book-title validation.
* Author existence validation before creating or updating books.

### ✍️ Author Management

* Create, read, update, and delete authors.
* Search authors by:

  * Name
  * Biography
* Sort authors by:

  * Name
  * Created date
* Pagination support.
* Duplicate author-name validation.
* Proper domain exceptions for missing authors.

### 🔐 Authentication & Authorization

The API uses **ASP.NET Core Identity** with GUID-based users and roles.

Supported authentication operations:

* User registration
* User login
* JWT access-token generation
* Refresh-token rotation
* Refresh-token revocation
* Role-based authorization
* User roles:

  * `User`
  * `Admin`

### 🎫 JWT Authentication

JWT access tokens contain:

* User ID
* Username
* Email
* Full name
* User roles

Access tokens are signed using a symmetric security key and include configurable:

* Issuer
* Audience
* Expiration time

### 🔄 Refresh Token System

Refresh tokens are:

* Cryptographically generated
* Stored in the database
* Associated with a specific user
* Expirable
* Revocable
* Rotated when used

The system tracks:

* `CreatedAt`
* `ExpiresAt`
* `RevokedAt`

A refresh token is considered active only when it is neither expired nor revoked.

---

## 🏗️ Architecture

The project follows a layered architecture that separates responsibilities between different parts of the application.

```text
BookAPI
│
├── Controllers
│
├── Data
│   ├── ApplicationDBContext
│   └── DatabaseSeeder
│
├── DTOs
│
├── Entity
│   ├── ApplicationUser
│   ├── Author
│   ├── Book
│   ├── RefreshToken
│   └── AccessToken
│
├── Exceptions
│
├── Parameters
│
├── Profiles
│   ├── AuthorProfile
│   └── BookProfile
│
├── Responses
│
├── Services
│   ├── AuthorService
│   ├── BookService
│   ├── AuthService
│   └── IServices
│
├── Validators
│
├── Middleware
│
└── Program.cs
```

### Responsibility Separation

```text
Controller
    ↓
Service
    ↓
Entity Framework Core
    ↓
SQL Server
```

Supporting components:

```text
DTOs
 ↓
AutoMapper

Request
 ↓
FluentValidation

Exception
 ↓
Global Exception Handler
 ↓
Standard API Response
```

---

## 🛠️ Technologies

| Technology                | Purpose                  |
| ------------------------- | ------------------------ |
| C#                        | Programming language     |
| .NET 10                   | Application framework    |
| ASP.NET Core Web API      | REST API                 |
| Entity Framework Core 10  | ORM                      |
| SQL Server                | Database                 |
| ASP.NET Core Identity     | User and role management |
| JWT Bearer Authentication | Authentication           |
| AutoMapper                | Object mapping           |
| FluentValidation          | Request validation       |
| Scalar                    | API documentation        |
| GUID                      | Entity identifiers       |
| LINQ                      | Querying                 |
| Async/Await               | Asynchronous operations  |

---

## 🗄️ Data Model

The main domain relationships are:

```text
ApplicationUser
      │
      │ 1
      │
      └─────────── *
              RefreshToken


Author
  │
  │ 1
  │
  └─────────── *
              Book
```

### Author

```text
Author
├── Id
├── Name
├── Bio
├── CreatedAt
└── Books
```

### Book

```text
Book
├── Id
├── Title
├── ISBN
├── Description
├── Price
├── Stock
├── PublishedAt
├── CreatedAt
├── UpdateAt
├── AuthorId
└── Author
```

### ApplicationUser

```text
ApplicationUser
├── Id
├── UserName
├── Email
├── FullName
└── RefreshTokens
```

### RefreshToken

```text
RefreshToken
├── Id
├── Token
├── CreatedAt
├── ExpiresAt
├── RevokedAt
├── UserId
└── User
```

---

## 📄 DTO Pattern

The API does not expose domain entities directly through its public API contract.

Instead, it uses **DTOs (Data Transfer Objects)**.

Examples:

```text
Book
 ├── BookDto
 ├── CreateBookDto
 └── UpdateBookDto

Author
 ├── AuthorDto
 ├── CreateAuthorDto
 └── UpdateAuthorDto

Authentication
 ├── RegisterDto
 ├── LoginDto
 ├── UserDto
 └── AuthResponseDto
```

This provides better separation between the internal domain model and the API contract.

---

## 🔍 Searching, Filtering & Sorting

The API supports flexible query parameters.

### Books

Example:

```http
GET /api/books?searchTerm=asp.net&pageNumber=1&pageSize=10
```

Filtering:

```http
GET /api/books?authorId={guid}
```

Date filtering:

```http
GET /api/books?publishedFrom=2025-01-01&publishedTo=2026-01-01
```

Sorting:

```http
GET /api/books?sortBy=price&sortDescending=true
```

### Authors

Example:

```http
GET /api/authors?searchTerm=martin&pageNumber=1&pageSize=10
```

Sorting:

```http
GET /api/authors?sortBy=name&sortDescending=false
```

---

## 📊 Pagination

Paginated endpoints return metadata such as:

```text
Items
PageNumber
PageSize
TotalCount
TotalPages
HasPreviousPage
HasNextPage
```

Example conceptual response:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 120,
  "totalPages": 12,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## ✅ Validation

The project uses **FluentValidation** for request and query validation.

Examples of validation rules include:

### Pagination

```text
PageNumber >= 1
PageSize between 1 and 100
```

### Sorting

Only predefined columns are accepted.

Books:

```text
title
price
stock
publishedat
createdat
```

Authors:

```text
name
createdat
```

### Date Range

```text
PublishedTo >= PublishedFrom
```

This prevents invalid query combinations before they reach the service layer.

---

## ⚠️ Exception Handling

The application uses custom domain exceptions for business-related errors.

Examples:

```text
AuthorNotFoundException
BookNotFoundException
DuplicateAuthorException
DuplicateBookException
```

The goal is to keep controllers clean and allow the global exception-handling layer to translate exceptions into appropriate HTTP responses.

---

## 📦 Standard API Response

The project provides a generic `ApiResponse<T>` structure.

It contains:

```text
StatusCode
Success
Message
Data
Errors
TimeStamp
```

Supported response factories include:

```text
Ok()
CreatedAt()
NoContent()
BadRequest()
NotFound()
Conflict()
InternalServerError()
```

This provides a consistent API response contract across endpoints.

---

## 🌱 Database Seeding

The project includes a `DatabaseSeeder` responsible for initializing application data.

The seeder creates:

### Roles

```text
User
Admin
```

### Authors

The database is populated with sample software-engineering authors.

### Books

The seeder generates sample books with:

* Randomized titles
* ISBN values
* Prices
* Stock quantities
* Publication dates
* Author relationships

A deterministic `Random` seed is used so generated test data remains reproducible.

---

## 🧩 Entity Framework Core

The application uses:

```text
Entity Framework Core 10
```

with:

```text
SQL Server
```

The `ApplicationDBContext` inherits from:

```text
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

This allows ASP.NET Core Identity and application entities to share the same database context.

Entity configurations are automatically discovered using:

```text
ApplyConfigurationsFromAssembly(...)
```

This keeps database configuration separated from the main `DbContext`.

---

## 🔐 Security

The authentication architecture is based on:

```text
ASP.NET Core Identity
        ↓
UserManager
        ↓
RoleManager
        ↓
JWT Access Token
        +
Refresh Token
```

### Access Token

Short-lived JWT token used to access protected endpoints.

### Refresh Token

Longer-lived token used to obtain a new access token without requiring the user to log in again.

### Refresh Token Rotation

When a refresh token is used:

```text
Old Refresh Token
       ↓
     Revoked
       ↓
New Access Token
       +
New Refresh Token
```

This reduces the risk associated with refresh-token reuse.

---

## 🧪 Example Authentication Flow

### Registration

```text
Client
  ↓
POST /api/auth/register
  ↓
Validate request
  ↓
Create Identity User
  ↓
Assign User role
  ↓
Generate JWT
  ↓
Generate Refresh Token
  ↓
Return authentication response
```

### Login

```text
Client
  ↓
POST /api/auth/login
  ↓
Find user
  ↓
Verify password
  ↓
Load roles
  ↓
Generate JWT
  ↓
Generate Refresh Token
  ↓
Return tokens
```

### Refresh

```text
Client
  ↓
POST /api/auth/refresh
  ↓
Validate Refresh Token
  ↓
Check expiration
  ↓
Check revocation
  ↓
Revoke old token
  ↓
Generate new token pair
```

---

## ⚙️ Configuration

The application requires configuration for:

```text
ConnectionStrings
JWT settings
```

Example structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "BookAPI",
    "Audience": "BookAPI.Client",
    "AccessTokenDurationInMinutes": 15
  }
}
```

> **Important:** Never commit real database credentials or JWT signing keys to source control.

For production environments, use environment variables, user secrets, Azure Key Vault, or another secure secret-management solution.

---

## ▶️ Getting Started

### 1. Clone the repository

```bash
git clone YOUR_REPOSITORY_URL
```

### 2. Navigate to the project

```bash
cd BookAPI
```

### 3. Configure the database

Update the connection string in the application configuration.

### 4. Configure JWT

Provide:

```text
Jwt:Key
Jwt:Issuer
Jwt:Audience
Jwt:AccessTokenDurationInMinutes
```

### 5. Apply migrations

```bash
dotnet ef database update
```

### 6. Run the application

```bash
dotnet run
```

The application will start using the configured ASP.NET Core environment.

---

## 📖 API Documentation

The project uses **Scalar** for API documentation and API exploration.

After starting the application, open the configured Scalar endpoint to explore:

* Authentication endpoints
* Book endpoints
* Author endpoints
* Request models
* Response models
* JWT authorization

---

## 🧱 Design Principles Demonstrated

This project was built to practice and demonstrate several important backend engineering concepts:

* Separation of Concerns
* Dependency Injection
* Service Layer
* DTO Pattern
* Entity/DTO separation
* Repository-independent service architecture
* Async programming
* LINQ
* IQueryable
* EF Core projection
* `AsNoTracking`
* AutoMapper
* FluentValidation
* Global exception handling
* Custom domain exceptions
* Pagination
* Filtering
* Searching
* Dynamic sorting
* Authentication
* Authorization
* JWT
* Refresh-token rotation
* ASP.NET Core Identity
* Role-based access control
* Database migrations
* Database seeding
* GUID-based identifiers

---

## 📈 Future Improvements

Possible future enhancements include:

* Unit tests
* Integration tests
* Testcontainers
* Advanced authorization policies
* Email verification
* Password reset
* Account lockout
* Token reuse detection
* Audit logging
* Soft delete
* Optimistic concurrency
* Caching
* Redis
* Rate limiting
* API versioning
* Background services
* Docker support
* CI/CD pipeline
* Structured logging
* OpenTelemetry
* Health checks
* Advanced search with full-text indexing

---

## 🎯 Project Goals

The main goal of **BookAPI** is not simply to create CRUD endpoints.

The project focuses on understanding how to build a maintainable **ASP.NET Core Web API** by combining:

```text
Domain Modeling
        +
Entity Framework Core
        +
ASP.NET Core Identity
        +
JWT Authentication
        +
Service Layer
        +
DTOs
        +
Validation
        +
Exception Handling
        +
Querying & Pagination
        =
Maintainable REST API
```

---

## 👨‍💻 Author

**Ahmed Othman**

C# / .NET Developer

This project was created as a practical backend engineering project to demonstrate modern **ASP.NET Core** development practices.

---

## 📄 License

This project is available for educational and portfolio purposes.
