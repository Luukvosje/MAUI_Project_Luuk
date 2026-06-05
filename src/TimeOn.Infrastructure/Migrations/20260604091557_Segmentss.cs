using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeOn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Segmentss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerVisits");

            migrationBuilder.DropTable(
                name: "RideSegments");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DrivingSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistanceMeters = table.Column<double>(type: "float", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrivingSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrivingSegments_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationarySegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterLatitude = table.Column<double>(type: "float", nullable: false),
                    CenterLongitude = table.Column<double>(type: "float", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistanceFromCustomerMeters = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: true),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationarySegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationarySegments_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrivingSegments_WorkSessionId",
                table: "DrivingSegments",
                column: "WorkSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StationarySegments_WorkSessionId",
                table: "StationarySegments",
                column: "WorkSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrivingSegments");

            migrationBuilder.DropTable(
                name: "StationarySegments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");

            migrationBuilder.CreateTable(
                name: "CustomerVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArrivalTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartureTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DistanceFromCustomerMeters = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSessionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerVisits_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerVisits_WorkSessions_WorkSessionId1",
                        column: x => x.WorkSessionId1,
                        principalTable: "WorkSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RideSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistanceMeters = table.Column<double>(type: "float", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSessionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RideSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RideSegments_WorkSessions_WorkSessionId",
                        column: x => x.WorkSessionId,
                        principalTable: "WorkSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RideSegments_WorkSessions_WorkSessionId1",
                        column: x => x.WorkSessionId1,
                        principalTable: "WorkSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisits_WorkSessionId",
                table: "CustomerVisits",
                column: "WorkSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisits_WorkSessionId1",
                table: "CustomerVisits",
                column: "WorkSessionId1");

            migrationBuilder.CreateIndex(
                name: "IX_RideSegments_WorkSessionId",
                table: "RideSegments",
                column: "WorkSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RideSegments_WorkSessionId1",
                table: "RideSegments",
                column: "WorkSessionId1");
        }
    }
}
