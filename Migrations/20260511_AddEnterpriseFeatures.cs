using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBDULICH.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Customer Segmentation Tables
            migrationBuilder.CreateTable(
                name: "CustomerSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SegmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Criteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerCount = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AverageSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageBookingFrequency = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LifetimeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChurnRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PreferredDestinations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredTourTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingPatterns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketingRecommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSegmentMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerSegmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    MatchingCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSegmentMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSegmentMembers_CustomerSegments_CustomerSegmentId",
                        column: x => x.CustomerSegmentId,
                        principalTable: "CustomerSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerSegmentMembers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    TotalSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageBookingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingFrequency = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LastBookingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaysSinceLastBooking = table.Column<int>(type: "int", nullable: false),
                    PreferredChannel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreferredPaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AverageAdvanceBookingDays = table.Column<int>(type: "int", nullable: false),
                    CancellationRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ReviewRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AverageReviewRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    ReferralCount = table.Column<int>(type: "int", nullable: false),
                    LifetimeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChurnRiskScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    EngagementScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LoyaltyScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BrowsingHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WishlistItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerBehaviors_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Availability Tables
            migrationBuilder.CreateTable(
                name: "Availabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TourId = table.Column<int>(type: "int", nullable: true),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    BookedCapacity = table.Column<int>(type: "int", nullable: false),
                    AvailableCapacity = table.Column<int>(type: "int", nullable: false),
                    HoldCapacity = table.Column<int>(type: "int", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AllowOverbooking = table.Column<bool>(type: "bit", nullable: false),
                    MaxOverbookingPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ViewsLast24Hours = table.Column<int>(type: "int", nullable: false),
                    BookingsLast24Hours = table.Column<int>(type: "int", nullable: false),
                    DemandLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Availabilities_Tour_TourId",
                        column: x => x.TourId,
                        principalTable: "Tour",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Availabilities_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvailabilityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_Availabilities_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalTable: "Availabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            // Create Indexes
            migrationBuilder.CreateIndex(
                name: "IX_CustomerSegments_SegmentType_IsActive",
                table: "CustomerSegments",
                columns: new[] { "SegmentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSegmentMembers_UserId_CustomerSegmentId",
                table: "CustomerSegmentMembers",
                columns: new[] { "UserId", "CustomerSegmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBehaviors_UserId",
                table: "CustomerBehaviors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBehaviors_ChurnRiskScore",
                table: "CustomerBehaviors",
                column: "ChurnRiskScore");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBehaviors_LifetimeValue",
                table: "CustomerBehaviors",
                column: "LifetimeValue");

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_EntityType_TourId_Date",
                table: "Availabilities",
                columns: new[] { "EntityType", "TourId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_EntityType_HotelId_Date",
                table: "Availabilities",
                columns: new[] { "EntityType", "HotelId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_Date_Status",
                table: "Availabilities",
                columns: new[] { "Date", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_UserId_Status",
                table: "AvailabilityBlocks",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_ExpiresAt",
                table: "AvailabilityBlocks",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomerSegmentMembers");
            migrationBuilder.DropTable(name: "CustomerBehaviors");
            migrationBuilder.DropTable(name: "CustomerSegments");
            migrationBuilder.DropTable(name: "AvailabilityBlocks");
            migrationBuilder.DropTable(name: "Availabilities");
        }
    }
}
