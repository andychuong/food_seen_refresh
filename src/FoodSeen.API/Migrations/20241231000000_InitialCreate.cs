using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FoodSeen.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    AuthProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DefaultLatitude = table.Column<double>(type: "double precision", nullable: true),
                    DefaultLongitude = table.Column<double>(type: "double precision", nullable: true),
                    DefaultRadiusKm = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Location = table.Column<Point>(type: "geography (point)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostCategories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCategories", x => new { x.CategoryId, x.PostId });
                    table.ForeignKey(
                        name: "FK_PostCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostCategories_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostCategories_PostId",
                table: "PostCategories",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_EventDate",
                table: "Posts",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Location",
                table: "Posts",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            // Seed Categories
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Description", "CreatedAt" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), "Free Food", "Completely free food events", DateTime.UtcNow },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), "Samples", "Free samples and tastings", DateTime.UtcNow },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), "BOGO", "Buy one get one free deals", DateTime.UtcNow },
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), "Community Event", "Community gatherings with food", DateTime.UtcNow },
                    { Guid.Parse("55555555-5555-5555-5555-555555555555"), "Restaurant Promo", "Restaurant promotions and deals", DateTime.UtcNow }
                });

            // Seed a test user
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Username", "PasswordHash", "FirstName", "LastName", "EmailConfirmed", "AuthProvider", "DefaultRadiusKm", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "demo@foodseen.com", "demo_user", null, "Demo", "User", true, "Local", 10, DateTime.UtcNow, DateTime.UtcNow
                });

            // Seed sample posts (San Francisco area)
            var post1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var post2Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var post3Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var post4Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

            migrationBuilder.Sql(@"
                INSERT INTO ""Posts"" (""Id"", ""UserId"", ""Title"", ""Description"", ""Address"", ""Latitude"", ""Longitude"", ""Location"", ""EventDate"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Free Pizza at Tech Meetup', 'Join us for our monthly tech meetup! Free pizza and drinks provided while networking with fellow developers.', '123 Market St, San Francisco, CA', 37.7935, -122.3960, ST_SetSRID(ST_MakePoint(-122.3960, 37.7935), 4326)::geography, NOW() + INTERVAL '7 days', true, NOW(), NOW()),
                ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Food Truck Festival', 'Annual food truck festival with free samples from over 20 food trucks. First 100 visitors get free tacos!', '500 Terry A Francois Blvd, San Francisco, CA', 37.7707, -122.3876, ST_SetSRID(ST_MakePoint(-122.3876, 37.7707), 4326)::geography, NOW() + INTERVAL '14 days', true, NOW(), NOW()),
                ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Bakery Grand Opening - Free Pastries', 'Celebrating our grand opening! Free coffee and pastry for all visitors this weekend.', '2100 Irving St, San Francisco, CA', 37.7636, -122.4819, ST_SetSRID(ST_MakePoint(-122.4819, 37.7636), 4326)::geography, NOW() + INTERVAL '3 days', true, NOW(), NOW()),
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Community BBQ in the Park', 'Free community BBQ event! Burgers, hot dogs, and vegetarian options available. Family friendly.', 'Golden Gate Park, San Francisco, CA', 37.7694, -122.4862, ST_SetSRID(ST_MakePoint(-122.4862, 37.7694), 4326)::geography, NOW() + INTERVAL '10 days', true, NOW(), NOW());
            ");

            // Link posts to categories
            migrationBuilder.InsertData(
                table: "PostCategories",
                columns: new[] { "CategoryId", "PostId" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }, // Free Food - Pizza
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }, // Community Event - Pizza
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") }, // Samples - Food Truck
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd") }, // Free Food - Bakery
                    { Guid.Parse("55555555-5555-5555-5555-555555555555"), Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd") }, // Restaurant Promo - Bakery
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") }, // Free Food - BBQ
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") }  // Community Event - BBQ
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostCategories");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
