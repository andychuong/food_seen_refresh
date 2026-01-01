# Food Seen 2025 - Task List

## Phase 1: Foundation ✅ COMPLETE

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

## Phase 2: Core Features ✅ COMPLETE

- [x] Implement UserRepository
- [x] Implement PostRepository with search and geospatial queries
- [x] Implement CategoryRepository
- [x] Create PostService with business logic
- [x] Create PostsController with CRUD and nearby endpoints
- [x] Create CategoriesController
- [x] Build bulletin board UI with post cards (including distance display)
- [x] Build post detail page with embedded map
- [x] Build create/edit post forms with location picker
- [x] Implement browser geolocation hook
- [x] Build map view page with event markers

## Phase 3: Authentication (Core Complete)

- [x] Implement email/password registration and login
- [x] Set up JWT token generation and validation
- [x] Implement refresh token storage and rotation
- [x] Create auth middleware
- [x] Build login/register UI components
- [x] Implement protected routes in React
- [x] Add auth context and hooks

### Future Auth Enhancements (Optional)

- [ ] Configure ASP.NET Core Identity (currently using custom implementation)
- [ ] Implement Google OAuth integration
- [ ] Implement Facebook OAuth integration
- [ ] Configure email service (SendGrid/AWS SES)
- [ ] Add password reset flow with email
- [ ] Add email verification flow
- [ ] Build social login buttons

## Phase 4: Location Features Polish ✅ COMPLETE

- [x] Add radius selector UI component
- [x] Implement "use my location" button with permission handling
- [x] Add map/list view toggle on home page
- [x] Handle location permission denied gracefully
- [x] Optimize geospatial queries with proper indexing (GIST index on Location column)

### Future Location Enhancements (Optional)

- [ ] Add default location settings to user profile page
- [ ] Implement location search/autocomplete (integrate geocoding API)

## Phase 5: Testing & Polish ✅ COMPLETE

- [x] Add pagination to post listing
- [x] Implement search and filtering (search bar + category filter)
- [x] Add form validation (frontend with field-level errors)
- [x] Add loading states and error handling
- [x] Implement toast notifications (success, error, warning, info)

### Future Testing Enhancements (Optional)

- [ ] Write unit tests for services and repositories
- [ ] Write integration tests for API endpoints (including geospatial)
- [ ] Test location features across different browsers
- [ ] Add backend validation with FluentValidation

## Phase 6: Deployment Preparation

- [ ] Production Docker configuration
- [ ] Environment variable management
- [ ] API documentation (Swagger/OpenAPI)
- [ ] README with setup instructions
