# Food Seen

A location-based platform for discovering and sharing free food events in your area. Built with .NET 10, React, and PostgreSQL with PostGIS for geospatial queries.

## Features

- **Location-Based Discovery**: Find free food events near you with distance-based sorting
- **Interactive Map View**: See all events on a map or browse as a list
- **Search & Filter**: Search by keywords and filter by category
- **User Authentication**: Register, login, and manage your own events
- **Event Management**: Create, edit, and delete food events with location picker

## Tech Stack

### Backend

- .NET 10 Web API
- Entity Framework Core 10 with PostgreSQL
- PostGIS for geospatial queries (NetTopologySuite)
- JWT authentication with refresh tokens
- FluentValidation for request validation

### Frontend

- React 18 with TypeScript
- Vite for fast development
- TailwindCSS with Shadcn/ui components
- React-Leaflet for interactive maps
- TanStack Query for data fetching

### Testing

- xUnit test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- WebApplicationFactory for integration tests

### Infrastructure

- Docker Compose for local development
- PostgreSQL with PostGIS extension

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/andychuong/food_seen_refresh.git
   cd food_seen_refresh
   ```

2. **Start the database**
   ```bash
   docker-compose up -d
   ```

3. **Run the API**
   ```bash
   cd src/FoodSeen.API
   dotnet run
   ```
   The API will be available at `http://localhost:5000`

4. **Run the frontend**
   ```bash
   cd src/food-seen-client
   npm install
   npm run dev
   ```
   The app will be available at `http://localhost:5173`

### Environment Configuration

#### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=foodseen;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "FoodSeen",
    "Audience": "FoodSeen",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}
```

#### Frontend (.env)
```env
VITE_API_URL=http://localhost:5000/api
```

## Project Structure

```
food_seen_2025/
├── src/
│   ├── FoodSeen.API/           # .NET 10 Web API
│   │   ├── Controllers/        # API endpoints
│   │   ├── Models/             # Entities, DTOs, Requests
│   │   ├── Services/           # Business logic
│   │   ├── Repositories/       # Data access
│   │   ├── Infrastructure/     # Auth, DbContext, Constants
│   │   └── Validators/         # FluentValidation validators
│   │
│   ├── FoodSeen.Tests/         # Test project
│   │   ├── Services/           # Unit tests for services
│   │   └── Integration/        # Integration tests for controllers
│   │
│   └── food-seen-client/       # React frontend
│       ├── src/
│       │   ├── components/     # UI components
│       │   ├── pages/          # Page components
│       │   ├── context/        # React contexts
│       │   ├── hooks/          # Custom hooks
│       │   ├── services/       # API client
│       │   └── types/          # TypeScript types
│       └── ...
│
├── docker-compose.yml          # Local development setup
└── tasklist.md                 # Development progress
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Create new account
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Revoke refresh token
- `GET /api/auth/me` - Get current user (requires auth)

### Posts
- `GET /api/posts` - List all posts (paginated)
- `GET /api/posts/nearby` - Get posts near location
- `GET /api/posts/search` - Search posts
- `GET /api/posts/{id}` - Get post by ID
- `POST /api/posts` - Create post (requires auth)
- `PUT /api/posts/{id}` - Update post (requires auth)
- `DELETE /api/posts/{id}` - Delete post (requires auth)

### Categories
- `GET /api/categories` - List all categories

## Development

### Running Tests

Run all tests from the solution root:

```bash
dotnet test
```

The test suite includes:

- **Unit Tests** (22 tests): PostService and AuthService business logic
- **Integration Tests** (11 tests): Full HTTP request/response pipeline testing

Tests use an in-memory database for isolation and fast execution.

### Building for Production

**API:**
```bash
cd src/FoodSeen.API
dotnet publish -c Release
```

**Frontend:**
```bash
cd src/food-seen-client
npm run build
```

### Database Migrations

Migrations run automatically on startup. To create a new migration:
```bash
cd src/FoodSeen.API
dotnet ef migrations add MigrationName
```

## Seed Data

The application comes with seed data for development:
- 5 categories: Free Food, Samples, BOGO, Community Event, Restaurant Promo
- Sample posts in the San Francisco area

## License

MIT
