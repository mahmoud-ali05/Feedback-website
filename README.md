# Feedback Website

A comprehensive ASP.NET Core MVC application for users to submit, read, and interact with product/service reviews. Users can create accounts, post reviews with ratings and images, like reviews, comment on reviews, and manage their profiles.

## Table of Contents
- [Project Overview](#project-overview)
- [Project Structure](#project-structure)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Setup and Installation](#setup-and-installation)
- [How Components Connect](#how-components-connect)
- [Controllers](#controllers)
- [Models](#models)
- [Services](#services)
- [Views](#views)
- [Data Access](#data-access)
- [Authentication & Authorization](#authentication--authorization)
- [File Upload Handling](#file-upload-handling)

## Project Overview

The Feedback website follows a typical ASP.NET Core MVC architecture with clear separation of concerns. It allows users to:
- Register and authenticate accounts
- Create, read, update, and delete reviews
- Like and unlike reviews
- Comment on reviews
- Manage their profiles (update bio, profile picture)
- Search and filter reviews by category

## Project Structure

```
Feedback/
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   └── ReviewsController.cs
├── Models/
│   ├── Users.cs
│   ├── Review.cs
│   ├── Comment.cs
│   ├── ReviewLike.cs
│   ├── CommentViewModel.cs
│   ├── ReviewCardViewModel.cs
│   ├── ReviewDetailViewModel.cs
│   ├── ErrorViewModel.cs
│   └── ... (other viewmodels)
├── Services/
│   └── ReviewService.cs
├── Data/
│   └── appDbContext.cs
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── Profile.cshtml
│   │   ├── ChangePassword.cshtml
│   │   └── VerifyEmail.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Reviews/
│   │   ├── Index.cshtml (Feed)
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── ...
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   └── ...
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ProfileViewModel.cs
│   ├── ChangePasswordViewModel.cs
│   ├── VerifyEmailViewModel.cs
│   └── ...
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── uploads/ (user-uploaded content: avatars, review images)
├── Migrations/
│   └── ... (Entity Framework Core migrations)
├── appsettings.json
├── appsettings.Development.json
├── Feedback.csproj
└── Program.cs
```

## Key Features

- **User Authentication**: Registration, login, logout, email verification, password change
- **Review Management**: Create, read, update, delete reviews with ratings (1-5)
- **Media Support**: Upload images for reviews and profile pictures
- **Interaction System**: Like/unlike reviews, comment on reviews
- **Personalization**: User profiles showing their reviews, liked reviews, and comments
- **Search & Filter**: Find reviews by text search or category filtering
- **Responsive Design**: Bootstrap-based UI that works on various screen sizes
- **Security**: Authorization checks ensuring users can only modify their own content

## Technology Stack

- **Framework**: ASP.NET Core MVC 8.0
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Views, Bootstrap, JavaScript/jQuery
- **Database**: SQL Server (EF Core Migrations)
- **File Storage**: Local file system for user uploads
- **Dependency Injection**: Built-in ASP.NET Core DI container

## Setup and Installation

1. **Prerequisites**:
   - .NET 8.0 SDK
   - SQL Server (or SQL Server Express)
   - Git

2. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd Feedback-website
   ```

3. **Configure the database**:
   - Update `appsettings.json` with your SQL Server connection string
   - Apply migrations:
     ```bash
     dotnet ef database update
     ```

4. **Run the application**:
   ```bash
   dotnet run
   ```
   or
   ```bash
   dotnet watch run
   ```

5. **Access the application**:
   - Navigate to `https://localhost:5001` (or the port shown in console)

## How Components Connect

### Data Flow
1. HTTP Request arrives at ASP.NET Core routing system
2. Controller action is invoked based on URL and HTTP verb
3. Controller dependencies (services, context, managers) are provided via Dependency Injection
4. Controller may call Service methods to perform business logic
5. Service methods interact with ApplicationDbContext to query/manipulate data
6. ApplicationDbContext maps entity changes to database tables via EF Core
7. Controller prepares a ViewModel (or uses model directly) and passes it to a View
8. View renders HTML using Razor syntax, displaying data from the model/viewmodel
9. HTML Response is sent back to client

### Specific Interactions

#### Review Creation Flow
1. User navigates to `/Reviews/Create` (GET) → ReviewsController.Create() returns Create.cshtml view
2. User submits form (POST) → ReviewsController.Create(Review review, IFormFile? Image)
3. Controller validates model, gets current user ID, sets review.UserId
4. Controller calls `_reviewService.AddReview(review, Image)`
5. Service:
   - Sets review.Date = DateTime.Now
   - If image provided: saves file to wwwroot/uploads/reviews/, sets review.ImageUrl
   - Adds review to context.Reviews and saves changes
6. Controller redirects to Index action

#### Displaying Review Feed Flow
1. User navigates to `/Reviews/Index` → ReviewsController.Index(searchString, categoryFilter)
2. Controller calls `_reviewService.GetAllReviewsAsync()` → returns IEnumerable<Review> with Includes
3. Controller applies search/filter filters in-memory (Linq to Objects)
4. Controller gets current user and builds HashSet of liked review IDs from context.ReviewLikes
5. Controller projects each review to ReviewCardViewModel (includes IsLikedByCurrentUser flag)
6. Controller passes list of ReviewCardViewModels to Views/Reviews/Feed.cshtml
7. View iterates over model, rendering partial _ReviewCard for each item
8. Partial displays review info, like button (with liked state), etc.

#### Liking a Review Flow
1. User clicks like button → AJAX POST to /Reviews/Like/{id}
2. ReviewsController.Like(id) called
3. Controller gets current user ID
4. Controller calls `_reviewService.LikeReview(id, userId)`
5. Service:
   - Finds review by id
   - Checks for existing like by this user on this review
   - If exists: removes like, decrements review.Likes
   - If not exists: adds like, increments review.Likes
   - Saves changes
   - Returns (newLikeCount, isNowLikedByCurrentUser, success)
6. Controller returns JSON result to AJAX call
7. JavaScript updates UI with new like count and toggles button state

#### Profile View Flow
1. User navigates to `/Account/Profile` → AccountsController.Profile()
2. Controller gets current user via userManager.GetUserAsync(User)
3. Controller queries:
   - MyReviews: context.Reviews.Where(r => r.UserId == user.Id).Include(r => r.Comments).OrderByDescending(r => r.Date)
   - LikedReviews: context.ReviewLikes.Where(rl => rl.UserId == user.Id).Include(rl => rl.Review).ThenInclude(r => r.User).Select(rl => rl.Review)
   - MyComments: context.Comments.Where(c => c.UserId == user.Id).Include(c => c.Review)
4. Controller builds ProfileViewModel with these collections and the user object
5. Controller passes model to Views/Account/Profile.cshtml
6. View displays user info, tabs for my reviews, liked reviews, my comments; each section iterates over respective collection

## Controllers

### AccountController (`Controllers/AccountController.cs`)
Handles user authentication, registration, profile management, and comment deletion.

**Dependencies:**
- `SignInManager<Users>` - ASP.NET Core Identity for sign-in operations
- `UserManager<Users>` - ASP.NET Core Identity for user management
- `ApplicationDbContext` - Entity Framework Core database context
- `IWebHostEnvironment` - Provides access to web root path for file operations

**Key Actions:**
- `Profile()`: Displays user's profile with their reviews, liked reviews, and comments
- `UpdateProfile(string? bio, IFormFile? photo)`: Updates user bio and profile photo (deletes old photo if exists)
- `DeleteProfilePhoto()`: Removes user's profile photo
- `DeleteComment(int id)`: Deletes a comment if the user owns it
- Login/Register/VerifyEmail/ChangePassword/Logout: Standard authentication flows

### HomeController (`Controllers/HomeController.cs`)
Simple controller for home page and error handling.

**Dependencies:**
- `ILogger<HomeController>` - Logging

**Key Actions:**
- `Index()`: Redirects to reviews feed (`Reviews/Index`)
- `Privacy()`: Returns privacy view
- `Error()`: Returns error view with request ID

### ReviewsController (`Controllers/ReviewsController.cs`)
Handles review creation, editing, deletion, liking, commenting, and displaying feeds.

**Dependencies:**
- `ReviewService` - Service layer for review operations
- `UserManager<Users>` - ASP.NET Core Identity for user operations
- `ApplicationDbContext` - Entity Framework Core database context

**Key Actions:**
- `Index(string searchString, string categoryFilter)`: Displays feed of reviews with search/filter capabilities
- `Details(int id)`: Shows detailed view of a specific review with comments and like status
- `AddComment(CommentViewModel vm)`: Handles comment submission (POST)
- `Create()`: GET for review creation form
- `Create(Review review, IFormFile? Image)`: POST for review creation (handles image upload)
- `Like(int id)`: AJAX endpoint for liking/unliking a review
- `Edit(int id)`: GET for review edit form (checks ownership)
- `Edit(int id, Review review, IFormFile? Image)`: POST for review update (handles image replacement)
- `Delete(int id)`: POST for review deletion (checks ownership)

## Models

### Users (`Models/Users.cs`)
Extends `IdentityUser` to add profile fields:
- `FullName`: User's display name
- `Bio`: Optional biography
- `ProfilePhotoUrl`: URL to user's avatar image
- Navigation properties:
  - `ReviewLikes`: Collection of likes made by this user
  - `Comments`: Collection of comments made by this user

### Review (`Models/Review.cs`)
Represents a product/service review:
- `Id`: Primary key
- `Rating`: Integer 1-5 (required, range validated)
- `ProductName`: Name of product being reviewed (required, max 100 chars)
- `Category`: Category of product (required, max 50 chars)
- `Text`: Review content (required, max 1000 chars)
- `ImageUrl`: Optional URL to uploaded image
- `ExternalLink`: Optional URL to external product/link
- `UserId`: Foreign key to Users
- `User`: Navigation property to Users
- `Date`: Timestamp of review creation
- `Likes`: Count of likes (denormalized for performance)
- Navigation properties:
  - `ReviewLikes`: Collection of likes on this review
  - `Comments`: Collection of comments on this review

### Comment (`Models/Comment.cs`)
Represents a comment on a review:
- `Id`: Primary key
- `Text`: Comment content
- `ReviewId`: Foreign key to Review
- `UserId`: Foreign key to Users
- Navigation properties:
  - `Review`: Navigation property to Review
  - `User`: Navigation property to Users

### ReviewLike (`Models/ReviewLike.cs`)
Represents a many-to-many relationship between Users and Reviews for likes:
- `ReviewId`: Foreign key to Review
- `UserId`: Foreign key to Users
- Navigation properties:
  - `Review`: Navigation property to Review
  - `User`: Navigation property to User

### ViewModels
Used to shape data specifically for views, often combining multiple models or adding view-specific properties.

- `LoginViewModel.cs`: Email, Password, RememberMe
- `RegisterViewModel.cs`: Name, Email, Password
- `ProfileViewModel.cs`: Users User, List<Review> MyReviews, List<Review> LikedReviews, List<Comment> MyComments
- `ChangePasswordViewModel.cs`: Email, NewPassword
- `VerifyEmailViewModel.cs`: Email
- `CommentViewModel.cs`: Text, ReviewId
- `ReviewCardViewModel.cs`: Review Review, bool IsLikedByCurrentUser
- `ReviewDetailViewModel.cs`: Review Review, CommentViewModel NewComment, bool IsLikedByCurrentUser
- `ErrorViewModel.cs`: RequestId

## Services

### ReviewService (`Services/ReviewService.cs`)
Encapsulates business logic for review operations, separating concerns from controllers.

**Dependencies:**
- `ApplicationDbContext` - Entity Framework Core database context
- `IWebHostEnvironment` - Provides access to web root path for file operations

**Key Methods:**
- `GetAllReviewsAsync()`: Retrieves all reviews with eager loading of User and Comments (including Comment.User), ordered by date descending
- `AddReview(Review review, IFormFile? imageFile)`: Sets Date, handles image upload (saves to wwwroot/uploads/reviews/), adds review to context
- `UpdateReview(Review review, IFormFile? imageFile)`: Handles image replacement (deletes old image if exists), updates review in context
- `DeleteReview(int id, string userId)`: Deletes review if userId matches review's UserId, also deletes associated image file
- `LikeReview(int id, string? userId)`: Toggles like status for a review by user:
  - If like exists: removes it and decrements Likes count
  - If like doesn't exist: adds it and increments Likes count
  - Returns tuple: (currentLikesCount, isNowLikedByUser, success)

## Data Access

### ApplicationDbContext (`Data/appDbContext.cs`)
Entity Framework Core database context that manages database connections and maps entities to tables.

**Inherits from:** `IdentityDbContext<Users>` (provides ASP.NET Core Identity tables)

**DbSets:**
- `Reviews`: DbSet<Review>
- `Comments`: DbSet<Comment>
- `ReviewLikes`: DbSet<ReviewLike>

**Configuration:** Uses standard EF Core conventions; relationships are defined via navigation properties and foreign keys in models.

## Views

Views are Razor pages (.cshtml) that generate HTML. They are organized by controller name under Views/.

### Account Views (`Views/Account/`)
- `Login.cshtml`: Form for user login
- `Register.cshtml`: Form for user registration
- `Profile.cshtml`: Displays user's profile, reviews, liked reviews, comments; includes forms for updating bio/photo and deleting comments
- `ChangePassword.cshtml`: Form for changing password
- `VerifyEmail.cshtml`: Form for verifying email before password reset

### Home Views (`Views/Home/`)
- `Index.cshtml`: Likely redirects or shows landing page (controller redirects to Reviews/Index)
- `Privacy.cshtml`: Privacy policy page

### Reviews Views (`Views/Reviews/`)
- `Feed.cshtml` (Index view): Shows search/filter form and list of review cards using partial `_ReviewCard`
- `Details.cshtml`: Shows full review details, comment form, and list of comments
- `Create.cshtml`: Form for creating a new review (includes image upload)
- `Edit.cshtml`: Form for editing an existing review (pre-populated, includes image replacement)

### Shared Views (`Views/Shared/`)
- `_Layout.cshtml`: Master layout defining common HTML structure (header, footer, scripts)
- `_ViewImports.cshtml`: Common imports and tag helpers for all views
- `_ViewStart.cshtml`: Sets layout for views

### Partials
- `_ReviewCard.cshtml`: Reusable component for displaying a review summary card (used in Feed.cshtml)

## Authentication & Authorization

- Uses ASP.NET Core Identity via UserManager<Users> and SignInManager<Users>
- `[Authorize]` attribute restricts actions to authenticated users
- Controllers check ownership:
  - For reviews: review.UserId must match current user's Id
  - For comments: comment.UserId must match current user's Id
  - Unauthorized attempts return Forbid() or Unauthorized()

## File Upload Handling

- When uploading images (profile photo or review image):
  - Controller receives IFormFile parameter
  - Controller passes it to service method
  - Service:
    - Combines _env.WebRootPath with uploads subfolder (avatars or reviews)
    - Creates directory if needed
    - Generates unique filename (GUID + original filename)
    - Saves file stream to disk
    - Sets entity's ImageUrl property to relative path (e.g., "/uploads/avatars/guid_filename.ext")
- When updating/deleting:
  - Service checks if existing ImageUrl is not null/empty
  - Maps relative path to physical path using _env.WebRootPath
  - Deletes file if exists before saving new one or removing reference

## Summary

The Feedback website follows a typical ASP.NET Core MVC architecture with clear separation of concerns:
- **Controllers** handle HTTP requests and coordinate between services and views
- **Services** encapsulate business logic and data operations
- **Models** represent domain entities and their relationships
- **ViewModels** shape data specifically for view consumption
- **Views** render HTML using Razor syntax and layout templates
- **Data Access** is handled by Entity Framework Core via ApplicationDbContext
- **Dependency Injection** provides loose coupling and testability
- **File System** is used for user-uploaded content (avatars, review images) with proper cleanup on update/delete

This structure makes the application maintainable, scalable, and follows established best practices for ASP.NET Core development.