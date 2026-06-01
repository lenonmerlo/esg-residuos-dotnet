using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsgResiduos.Api.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "CollectionPoints",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CapacityKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                AlertVolumeKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                OccupiedVolumeKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CollectionPoints", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "WasteTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                WasteCategory = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_WasteTypes", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "Collection",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CollectionPointId = table.Column<int>(type: "int", nullable: false),
                WasteTypeId = table.Column<int>(type: "int", nullable: false),
                CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                VolumeKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                DestinatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DestinationHistory = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Collection", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Collection_CollectionPoints_CollectionPointId",
                    column: x => x.CollectionPointId,
                    principalTable: "CollectionPoints",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_Collection_WasteTypes_WasteTypeId",
                    column: x => x.WasteTypeId,
                    principalTable: "WasteTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "CollectionAlerts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CollectionPointId = table.Column<int>(type: "int", nullable: false),
                CollectionId = table.Column<int>(type: "int", nullable: true),
                AlertedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                AlertType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_CollectionAlerts", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_CollectionAlerts_CollectionPoints_CollectionPointId",
                    column: x => x.CollectionPointId,
                    principalTable: "CollectionPoints",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_CollectionAlerts_Collection_CollectionId",
                    column: x => x.CollectionId,
                    principalTable: "Collection",
                    principalColumn: "Id");
            });

        _ = migrationBuilder.CreateTable(
            name: "Destination",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CollectionId = table.Column<int>(type: "int", nullable: false),
                DestinatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                DestinationName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ProcessingType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                DestinatedVolumeKg = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Destination", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Destination_Collection_CollectionId",
                    column: x => x.CollectionId,
                    principalTable: "Collection",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Collection_CollectionPointId",
            table: "Collection",
            column: "CollectionPointId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Collection_WasteTypeId",
            table: "Collection",
            column: "WasteTypeId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_CollectionAlerts_CollectionId",
            table: "CollectionAlerts",
            column: "CollectionId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_CollectionAlerts_CollectionPointId",
            table: "CollectionAlerts",
            column: "CollectionPointId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Destination_CollectionId",
            table: "Destination",
            column: "CollectionId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_WasteTypes_WasteCategory",
            table: "WasteTypes",
            column: "WasteCategory",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "CollectionAlerts");

        _ = migrationBuilder.DropTable(
            name: "Destination");

        _ = migrationBuilder.DropTable(
            name: "Users");

        _ = migrationBuilder.DropTable(
            name: "Collection");

        _ = migrationBuilder.DropTable(
            name: "CollectionPoints");

        _ = migrationBuilder.DropTable(
            name: "WasteTypes");
    }
}