using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBDULICH.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppleId",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Tour",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    TourId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TravelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SpecialRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Tour_TourId",
                        column: x => x.TourId,
                        principalTable: "Tour",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    TicketCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassengerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PassengerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PassengerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_Orders_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tour_CategoryId",
                table: "Tour",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_TourId",
                table: "OrderDetail",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_BookingId_Status",
                table: "Ticket",
                columns: new[] { "BookingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TicketCode",
                table: "Ticket",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ValidUntil",
                table: "Ticket",
                column: "ValidUntil");

            migrationBuilder.AddForeignKey(
                name: "FK_Tour_Category_CategoryId",
                table: "Tour",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tour_Category_CategoryId",
                table: "Tour");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Tour_CategoryId",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "AppleId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FacebookId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Orders");
        }
    }
}
