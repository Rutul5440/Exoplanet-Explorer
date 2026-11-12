using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExoplanetExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exoplanets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanetName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiscoveryMethod = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DiscoveryYear = table.Column<int>(type: "int", nullable: true),
                    MassEarth = table.Column<double>(type: "float", nullable: true),
                    RadiusEarth = table.Column<double>(type: "float", nullable: true),
                    OrbitalPeriodDays = table.Column<double>(type: "float", nullable: true),
                    SemiMajorAxisAu = table.Column<double>(type: "float", nullable: true),
                    SemiMajorAxisDerived = table.Column<bool>(type: "bit", nullable: false),
                    EquilibriumTempK = table.Column<double>(type: "float", nullable: true),
                    StellarLuminosityLogSolar = table.Column<double>(type: "float", nullable: true),
                    StellarLuminositySolar = table.Column<double>(type: "float", nullable: true),
                    LuminosityDerived = table.Column<bool>(type: "bit", nullable: false),
                    StellarTeffK = table.Column<double>(type: "float", nullable: true),
                    StellarRadiusSolar = table.Column<double>(type: "float", nullable: true),
                    StellarMassSolar = table.Column<double>(type: "float", nullable: true),
                    DistanceParsecs = table.Column<double>(type: "float", nullable: true),
                    HzInnerConservativeAu = table.Column<double>(type: "float", nullable: true),
                    HzOuterConservativeAu = table.Column<double>(type: "float", nullable: true),
                    HzInnerOptimisticAu = table.Column<double>(type: "float", nullable: true),
                    HzOuterOptimisticAu = table.Column<double>(type: "float", nullable: true),
                    InsolationEarth = table.Column<double>(type: "float", nullable: true),
                    HabitabilityStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsRockyCandidate = table.Column<bool>(type: "bit", nullable: false),
                    EarthSimilarityScore = table.Column<double>(type: "float", nullable: true),
                    ContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LastSyncedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exoplanets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggerType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    UnchangedCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_DiscoveryMethod",
                table: "Exoplanets",
                column: "DiscoveryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_DiscoveryYear",
                table: "Exoplanets",
                column: "DiscoveryYear");

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_Habitability_Rocky",
                table: "Exoplanets",
                columns: new[] { "HabitabilityStatus", "IsRockyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_HostName",
                table: "Exoplanets",
                column: "HostName");

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_MassEarth",
                table: "Exoplanets",
                column: "MassEarth");

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_OrbitalPeriodDays",
                table: "Exoplanets",
                column: "OrbitalPeriodDays");

            migrationBuilder.CreateIndex(
                name: "IX_Exoplanets_RadiusEarth",
                table: "Exoplanets",
                column: "RadiusEarth");

            migrationBuilder.CreateIndex(
                name: "UX_Exoplanets_PlanetName",
                table: "Exoplanets",
                column: "PlanetName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_StartedUtc",
                table: "SyncRuns",
                column: "StartedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exoplanets");

            migrationBuilder.DropTable(
                name: "SyncRuns");
        }
    }
}
