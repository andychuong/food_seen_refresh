# Product Requirements Document: Food Seen Redesign

## Overview

**Project Name:** Food Seen 2025
**Version:** 2.0
**Last Updated:** December 31, 2025
**Status:** Phase 1 Complete

### Purpose
Food Seen is a community bulletin board application that enables users to share and discover free food events in their local area. Users can browse nearby events, see them on a map, and filter by distance. This document outlines the complete redesign of the original Node.js/Express application into a modern React frontend with a .NET backend architecture.

### Goals
- Modernize the tech stack for improved maintainability and scalability
- Implement SOLID principles in the backend architecture
- Provide a polished, accessible UI using React and Shadcn components
- Enable location-based discovery of food events near the user
- Maintain all existing functionality while enabling future feature expansion

---

## Technology Stack

### Frontend
| Technology | Purpose |
|------------|---------|
| React 18+ | UI framework |
| TypeScript | Type safety |
| Shadcn/ui | Component library |
| Tailwind CSS | Styling |
| React Router | Client-side routing |
| React Query (TanStack Query) | Server state management |
| Axios | HTTP client |
| Vite | Build tool |
| Leaflet / React-Leaflet | Interactive maps |
| Browser Geolocation API | User location detection |

### Backend
| Technology | Purpose |
|------------|---------|
| .NET 8 | Framework |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | ORM |
| PostgreSQL + PostGIS | Database with geospatial support |
| NetTopologySuite | .NET geospatial library |
| JWT | Authentication tokens |
| ASP.NET Core Identity | User management & password hashing |
| OAuth 2.0 (Google, Facebook) | Social authentication |

### Infrastructure
| Technology | Purpose |
|------------|---------|
| Docker | Containerization |
| Docker Compose | Local development orchestration |
| Railway | Hosting (API, Database, Frontend) |

---

## Architecture

### Backend Architecture (SOLID Principles)

The backend follows a layered architecture with clear separation of concerns:

```
FoodSeen.API/
├── Controllers/           # API endpoints - handle HTTP requests/responses
├── Models/
│   ├── Entities/         # Database entities
│   ├── DTOs/             # Data Transfer Objects
│   └── Requests/         # Request models
├── Repositories/
│   ├── Interfaces/       # Repository contracts
│   └── Implementations/  # Repository implementations
├── Services/
│   ├── Interfaces/       # Service contracts
│   └── Implementations/  # Business logic implementations
├── Infrastructure/
│   ├── Data/             # DbContext and configurations
│   └── Auth/             # Authentication handlers
└── Program.cs            # Application entry point
```

#### SOLID Implementation

**S - Single Responsibility Principle**
- Controllers: Only handle HTTP request/response mapping
- Services: Contain business logic only
- Repositories: Handle data access only

**O - Open/Closed Principle**
- Use interfaces for all services and repositories
- New functionality added through new implementations, not modifications

**L - Liskov Substitution Principle**
- All implementations are interchangeable with their interfaces
- Mock implementations for testing

**I - Interface Segregation Principle**
- Separate interfaces for different repository operations
- Granular service interfaces

**D - Dependency Inversion Principle**
- All dependencies injected via constructor injection
- High-level modules depend on abstractions (interfaces)

### Frontend Architecture

```
food-seen-client/
├── src/
│   ├── components/
│   │   ├── ui/           # Shadcn components
│   │   ├── layout/       # Layout components
│   │   └── features/     # Feature-specific components
│   ├── pages/            # Route pages
│   ├── hooks/            # Custom hooks
│   ├── services/         # API service layer
│   ├── types/            # TypeScript types
│   ├── lib/              # Utilities
│   └── context/          # React context providers
├── public/
└── index.html
```

---

## Data Models

### Database Schema

#### Users Table
| Column | Type | Constraints |
|--------|------|-------------|
| id | UUID | PRIMARY KEY |
| email | VARCHAR(255) | UNIQUE, NOT NULL |
| password_hash | VARCHAR(255) | NULL (for OAuth users) |
| username | VARCHAR(100) | NOT NULL |
| first_name | VARCHAR(100) | |
| last_name | VARCHAR(100) | |
| avatar_url | VARCHAR(500) | |
| email_confirmed | BOOLEAN | DEFAULT FALSE |
| auth_provider | VARCHAR(50) | DEFAULT 'local' |
| provider_id | VARCHAR(255) | NULL |
| default_latitude | DECIMAL(10,8) | NULL |
| default_longitude | DECIMAL(11,8) | NULL |
| default_radius_km | INTEGER | DEFAULT 10 |
| created_at | TIMESTAMP | DEFAULT NOW() |
| updated_at | TIMESTAMP | DEFAULT NOW() |

#### Posts Table
| Column | Type | Constraints |
|--------|------|-------------|
| id | UUID | PRIMARY KEY |
| user_id | UUID | FOREIGN KEY -> Users |
| title | VARCHAR(200) | NOT NULL |
| description | TEXT | NOT NULL |
| address | VARCHAR(300) | NOT NULL |
| latitude | DECIMAL(10,8) | NOT NULL |
| longitude | DECIMAL(11,8) | NOT NULL |
| location | GEOGRAPHY(Point, 4326) | NOT NULL (PostGIS spatial index) |
| event_date | TIMESTAMP | NOT NULL |
| event_end_date | TIMESTAMP | |
| is_active | BOOLEAN | DEFAULT TRUE |
| created_at | TIMESTAMP | DEFAULT NOW() |
| updated_at | TIMESTAMP | DEFAULT NOW() |

*Note: The `location` column is a PostGIS geography point derived from latitude/longitude for efficient spatial queries. A spatial index (GIST) should be created on this column.*

#### Categories Table
| Column | Type | Constraints |
|--------|------|-------------|
| id | UUID | PRIMARY KEY |
| name | VARCHAR(50) | UNIQUE, NOT NULL |
| description | VARCHAR(200) | |

#### PostCategories Table (Junction)
| Column | Type | Constraints |
|--------|------|-------------|
| post_id | UUID | FOREIGN KEY -> Posts |
| category_id | UUID | FOREIGN KEY -> Categories |
| | | PRIMARY KEY (post_id, category_id) |

#### RefreshTokens Table

| Column | Type | Constraints |
|--------|------|-------------|
| id | UUID | PRIMARY KEY |
| user_id | UUID | FOREIGN KEY -> Users |
| token | VARCHAR(500) | UNIQUE, NOT NULL |
| expires_at | TIMESTAMP | NOT NULL |
| created_at | TIMESTAMP | DEFAULT NOW() |
| revoked_at | TIMESTAMP | NULL |

### Entity Models (.NET)

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailConfirmed { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public string? ProviderId { get; set; }
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public int DefaultRadiusKm { get; set; } = 10;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Post> Posts { get; set; }
}

public enum AuthProvider
{
    Local,
    Google,
    Facebook
}

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Point Location { get; set; }  // NetTopologySuite.Geometries.Point
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; }
    public ICollection<Category> Categories { get; set; }
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<Post> Posts { get; set; }
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; set; }
}
```

---

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register with email/password |
| POST | `/api/auth/login` | Login with email/password |
| POST | `/api/auth/google` | Authenticate with Google OAuth |
| POST | `/api/auth/facebook` | Authenticate with Facebook OAuth |
| POST | `/api/auth/refresh` | Refresh JWT token |
| POST | `/api/auth/logout` | Logout user |
| POST | `/api/auth/forgot-password` | Request password reset email |
| POST | `/api/auth/reset-password` | Reset password with token |
| POST | `/api/auth/verify-email` | Verify email address |
| GET | `/api/auth/me` | Get current user info |

### Posts
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/posts` | Get all active posts (paginated) |
| GET | `/api/posts/nearby` | Get posts within radius of coordinates |
| GET | `/api/posts/search` | Search posts by title/description |
| GET | `/api/posts/{id}` | Get post by ID |
| POST | `/api/posts` | Create new post (auth required) |
| PUT | `/api/posts/{id}` | Update post (owner only) |
| DELETE | `/api/posts/{id}` | Delete post (owner only) |
| GET | `/api/posts/user/{userId}` | Get posts by user |
| GET | `/api/posts/my` | Get current user's posts (auth required) |

#### Nearby Posts Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| latitude | double | required | User's current latitude |
| longitude | double | required | User's current longitude |
| radiusKm | int | 10 | Search radius in kilometers |
| page | int | 1 | Page number |
| pageSize | int | 20 | Results per page |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}/posts` | Get posts by category |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users/{id}` | Get user profile |
| PUT | `/api/users/{id}` | Update user profile (self only) |

---

## Interface Definitions

### Repository Interfaces

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetActivePostsAsync(int page, int pageSize);
    Task<IEnumerable<Post>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize);
    Task<IEnumerable<Post>> SearchPostsAsync(string query, int page, int pageSize);
    Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId);
    Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(Guid categoryId);
    Task<int> GetActivePostsCountAsync();
    Task<int> GetNearbyPostsCountAsync(double latitude, double longitude, int radiusKm);
    Task<int> GetSearchResultsCountAsync(string query);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByProviderIdAsync(AuthProvider provider, string providerId);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}
```

### Service Interfaces

```csharp
public interface IPostService
{
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PaginatedResult<PostDto>> GetActivePostsAsync(int page, int pageSize);
    Task<PaginatedResult<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize);
    Task<PaginatedResult<PostDto>> SearchPostsAsync(string query, int page, int pageSize);
    Task<IEnumerable<PostDto>> GetUserPostsAsync(Guid userId);
    Task<PostDto> CreatePostAsync(Guid userId, CreatePostRequest request);
    Task<PostDto> UpdatePostAsync(Guid userId, Guid postId, UpdatePostRequest request);
    Task DeletePostAsync(Guid userId, Guid postId);
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> AuthenticateWithGoogleAsync(string idToken);
    Task<AuthResult> AuthenticateWithFacebookAsync(string accessToken);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task VerifyEmailAsync(string token);
}

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
}

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token);
    Task SendPasswordResetAsync(string email, string token);
}
```

### DTOs (Data Transfer Objects)

```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public int DefaultRadiusKm { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorUsername { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceKm { get; set; }  // Populated when querying nearby posts
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public bool IsActive { get; set; }
    public List<string> Categories { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
}

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### Request Models

```csharp
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreatePostRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

public class UpdatePostRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public int? DefaultRadiusKm { get; set; }
}

public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
```

---

## Features & User Stories

### Authentication

- **US-001**: As a visitor, I can register with my email and password
- **US-002**: As a visitor, I can sign in with my email and password
- **US-003**: As a visitor, I can sign in with my Google account
- **US-004**: As a visitor, I can sign in with my Facebook account
- **US-005**: As a user, I can sign out of my account
- **US-006**: As a user, I remain logged in across sessions (token refresh)
- **US-007**: As a user, I can reset my password if I forget it
- **US-008**: As a user, I receive an email to verify my account

### Browsing Posts

- **US-009**: As a visitor, I can view all active food event posts on the bulletin board
- **US-010**: As a visitor, I can view post details including location and time
- **US-011**: As a visitor, I can filter posts by category
- **US-012**: As a visitor, I can search posts by title or description
- **US-013**: As a visitor, I can paginate through posts

### Location-Based Discovery

- **US-014**: As a user, I can allow the app to detect my current location
- **US-015**: As a user, I can see food events near my current location
- **US-016**: As a user, I can adjust the search radius (e.g., 5km, 10km, 25km)
- **US-017**: As a user, I can view nearby events on an interactive map
- **US-018**: As a user, I can see the distance to each event from my location
- **US-019**: As a user, I can save a default location in my profile
- **US-020**: As a user, I can manually enter a location to search around

### Managing Posts

- **US-021**: As an authenticated user, I can create a new food event post with a location
- **US-022**: As an authenticated user, I can edit my own posts
- **US-023**: As an authenticated user, I can delete my own posts
- **US-024**: As an authenticated user, I can view all my posts in one place
- **US-025**: As an authenticated user, I can mark my post as inactive
- **US-026**: As an authenticated user, I can pick a location on a map when creating a post

### User Profile

- **US-027**: As an authenticated user, I can view my profile
- **US-028**: As an authenticated user, I can update my profile information

---

## UI Components (Shadcn)

### Pages

1. **Home/Bulletin Board** - Grid of post cards with filtering and map toggle
2. **Map View** - Full-screen interactive map showing nearby events
3. **Post Detail** - Full post information with location map
4. **Create Post** - Form for new posts with location picker
5. **Edit Post** - Form for editing existing posts
6. **My Posts** - User's post management dashboard
7. **Profile** - User profile view/edit with default location settings
8. **Login** - Email/password login with social login options
9. **Register** - Email/password registration with social signup options
10. **Forgot Password** - Request password reset
11. **Reset Password** - Set new password with token

### Core Components

| Component | Shadcn Components Used |
|-----------|----------------------|
| Navigation | NavigationMenu, Avatar, DropdownMenu |
| Post Card | Card, Badge, Button |
| Post Form | Form, Input, Textarea, DatePicker, Select |
| Post List | Card (grid), Pagination, Skeleton |
| Filter Bar | Select, Input, Button, Slider (for radius) |
| Auth Button | Button, Avatar |
| Dialog/Modal | Dialog, AlertDialog |
| Toast Notifications | Toast |
| Location Picker | Custom (Leaflet map integration) |
| Event Map | Custom (Leaflet with markers) |
| Distance Badge | Badge |
| Radius Selector | Slider, Select |
| Location Permission | Dialog, Button |

---

## Non-Functional Requirements

### Performance
- API response time < 200ms for standard queries
- Frontend initial load < 3 seconds
- Support 100 concurrent users

### Security
- HTTPS only in production
- JWT tokens with 15-minute expiry
- Refresh tokens with 7-day expiry
- Input validation on all endpoints
- CORS configured for frontend origin only
- Rate limiting on API endpoints

### Accessibility
- WCAG 2.1 AA compliance
- Keyboard navigation support
- Screen reader compatible
- Color contrast ratios met

### Browser Support
- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Edge (latest 2 versions)

---

## Project Structure

```
food_seen_2025/
├── src/
│   ├── FoodSeen.API/              # .NET Web API project
│   │   ├── Controllers/
│   │   ├── Models/
│   │   │   ├── Entities/
│   │   │   ├── DTOs/
│   │   │   └── Requests/
│   │   ├── Repositories/
│   │   │   ├── Interfaces/
│   │   │   └── Implementations/
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   └── Implementations/
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   └── Auth/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   └── food-seen-client/          # React frontend
│       ├── src/
│       │   ├── components/
│       │   ├── pages/
│       │   ├── hooks/
│       │   ├── services/
│       │   ├── types/
│       │   ├── lib/
│       │   └── context/
│       ├── public/
│       ├── package.json
│       └── vite.config.ts
│
├── docker-compose.yml
├── .gitignore
├── README.md
└── prd.md
```

---

## Implementation Notes

### Phase 1 Implementation Details (December 31, 2025)

**Backend (.NET API)**
- API runs on port **5001** (changed from 5000 due to macOS port conflict)
- PostgreSQL with PostGIS extension running in Docker container
- Auto-migrations enabled in Program.cs for development
- GIST spatial index on Posts.Location column for efficient geospatial queries

**Database Seed Data**
- 5 categories: Community Event, Corporate Event, Restaurant, Grocery Store, Food Bank
- 1 demo user: demo@foodseen.com / password123
- 4 sample posts in San Francisco area with PostGIS geography points

**Frontend (React/Vite)**
- Vite dev server on port 5173 with proxy to API at localhost:5001
- Shadcn/ui components installed and configured
- TanStack Query for server state management
- Axios client with JWT interceptor configured

**Local Development**
- `docker-compose up` starts PostgreSQL/PostGIS + .NET API
- Frontend: `cd src/food-seen-client && npm run dev`
- API accessible at http://localhost:5001/api
- Frontend accessible at http://localhost:5173

---

## Development Phases

### Phase 1: Foundation ✅ COMPLETE

- [x] Set up .NET 8 Web API project structure
- [x] Configure Entity Framework Core with PostgreSQL + PostGIS
- [x] Install and configure NetTopologySuite for geospatial support
- [x] Define all entity models (User, Post, Category, RefreshToken)
- [x] Create database migrations for all entities (including spatial indexes)
- [x] Implement base generic repository pattern with interfaces
- [x] Set up React project with Vite and TypeScript
- [x] Configure Tailwind CSS and Shadcn/ui
- [x] Install and configure React-Leaflet for maps
- [x] Set up Docker Compose for local development (API + PostgreSQL/PostGIS)
- [x] Add seed data (5 categories, demo user, 4 sample posts in SF area)

### Phase 2: Core Features

- [ ] Implement UserRepository
- [ ] Implement PostRepository with search and geospatial queries
- [ ] Implement CategoryRepository
- [ ] Create PostService with business logic
- [ ] Create PostsController with CRUD and nearby endpoints
- [ ] Create CategoriesController
- [ ] Build bulletin board UI with post cards (including distance display)
- [ ] Build post detail page with embedded map
- [ ] Build create/edit post forms with location picker
- [ ] Implement browser geolocation hook
- [ ] Build map view page with event markers

### Phase 3: Authentication

- [ ] Configure ASP.NET Core Identity
- [ ] Implement email/password registration and login
- [ ] Set up JWT token generation and validation
- [ ] Implement refresh token storage and rotation
- [ ] Implement Google OAuth integration
- [ ] Implement Facebook OAuth integration
- [ ] Configure email service (SendGrid/AWS SES)
- [ ] Add password reset flow with email
- [ ] Add email verification flow
- [ ] Create auth middleware
- [ ] Build login/register UI components
- [ ] Build social login buttons
- [ ] Implement protected routes in React
- [ ] Add auth context and hooks

### Phase 4: Location Features Polish

- [ ] Add radius selector UI component
- [ ] Implement "use my location" button with permission handling
- [ ] Add default location settings to user profile page
- [ ] Implement location search/autocomplete (optional: integrate geocoding API)
- [ ] Add map/list view toggle on home page
- [ ] Optimize geospatial queries with proper indexing
- [ ] Handle location permission denied gracefully

### Phase 5: Testing & Polish

- [ ] Add pagination to post listing
- [ ] Implement search and filtering
- [ ] Add form validation (frontend and backend)
- [ ] Write unit tests for services and repositories
- [ ] Write integration tests for API endpoints (including geospatial)
- [ ] Add loading states and error handling
- [ ] Implement toast notifications
- [ ] Test location features across different browsers

### Phase 6: Deployment Preparation

- [ ] Production Docker configuration
- [ ] Environment variable management
- [ ] API documentation (Swagger/OpenAPI)
- [ ] README with setup instructions

---

## Success Metrics

| Metric | Target |
|--------|--------|
| All existing features preserved | 100% |
| API test coverage | > 80% |
| Lighthouse Performance Score | > 90 |
| Lighthouse Accessibility Score | > 90 |
| Build time | < 2 minutes |

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| OAuth provider configuration | Low | Google and Facebook have excellent .NET support via built-in providers |
| Email delivery for verification/reset | Medium | Use established email service (SendGrid, AWS SES) |
| Data migration from old system | Medium | Create migration scripts, validate data integrity |
| Learning curve for new stack | Low | Use well-documented technologies |
| User denies location permission | Medium | Provide manual location entry, default to city-level search |
| PostGIS complexity | Low | Well-documented, NetTopologySuite provides excellent abstraction |
| Geospatial query performance | Medium | Use proper spatial indexes (GIST), limit search radius |
| Map library bundle size | Low | Use dynamic imports for Leaflet components |

---

## Appendix

### Reference Links

- [Original Food Seen Repository](https://github.com/andychuong/food_seen)
- [Shadcn/ui Documentation](https://ui.shadcn.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostGIS Documentation](https://postgis.net/documentation/)
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)
- [EF Core Spatial Data](https://docs.microsoft.com/en-us/ef/core/modeling/spatial)
- [React-Leaflet Documentation](https://react-leaflet.js.org/)
- [Leaflet Documentation](https://leafletjs.com/reference.html)
- [Browser Geolocation API](https://developer.mozilla.org/en-US/docs/Web/API/Geolocation_API)
