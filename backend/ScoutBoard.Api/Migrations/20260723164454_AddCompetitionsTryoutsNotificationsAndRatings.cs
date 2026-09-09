using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoutBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionsTryoutsNotificationsAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "PlayerMatchStatistics",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "FootballMatches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClubTryouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClubId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1500, nullable: false),
                    TryoutDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Venue = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: true),
                    MinimumAge = table.Column<int>(type: "INTEGER", nullable: true),
                    MaximumAge = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubTryouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubTryouts_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Season = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competitions_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 700, nullable: false),
                    Link = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TryoutApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClubTryoutId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 700, nullable: false),
                    CoachNote = table.Column<string>(type: "TEXT", maxLength: 700, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TryoutApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TryoutApplications_ClubTryouts_ClubTryoutId",
                        column: x => x.ClubTryoutId,
                        principalTable: "ClubTryouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TryoutApplications_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionClubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompetitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClubId = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionClubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionClubs_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionClubs_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_CompetitionId",
                table: "FootballMatches",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubTryouts_ClubId",
                table: "ClubTryouts",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionClubs_ClubId",
                table: "CompetitionClubs",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionClubs_CompetitionId_ClubId",
                table: "CompetitionClubs",
                columns: new[] { "CompetitionId", "ClubId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_CreatedById",
                table: "Competitions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TryoutApplications_ClubTryoutId_PlayerProfileId",
                table: "TryoutApplications",
                columns: new[] { "ClubTryoutId", "PlayerProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TryoutApplications_PlayerProfileId",
                table: "TryoutApplications",
                column: "PlayerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FootballMatches_Competitions_CompetitionId",
                table: "FootballMatches",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FootballMatches_Competitions_CompetitionId",
                table: "FootballMatches");

            migrationBuilder.DropTable(
                name: "CompetitionClubs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TryoutApplications");

            migrationBuilder.DropTable(
                name: "Competitions");

            migrationBuilder.DropTable(
                name: "ClubTryouts");

            migrationBuilder.DropIndex(
                name: "IX_FootballMatches_CompetitionId",
                table: "FootballMatches");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlayerMatchStatistics");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "FootballMatches");
        }
    }
}
