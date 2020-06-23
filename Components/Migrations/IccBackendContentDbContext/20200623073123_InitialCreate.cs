using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.IccBackendContentDbContext
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfectionConfirmationCodes",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    BatchId = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    GeneratedBy = table.Column<string>(nullable: false),
                    Revoked = table.Column<DateTime>(nullable: true),
                    Used = table.Column<DateTime>(nullable: true),
                    UsedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfectionConfirmationCodes", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfectionConfirmationCodes_BatchId",
                table: "InfectionConfirmationCodes",
                column: "BatchId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InfectionConfirmationCodes");
        }
    }
}
