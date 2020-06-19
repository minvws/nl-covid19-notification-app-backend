using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.WorkflowDbContext
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "EntityFrameworkHiLoSequence",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "KeyReleaseWorkflowState",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LabConfirmationId = table.Column<string>(nullable: true),
                    ConfirmationKey = table.Column<string>(nullable: true),
                    BucketId = table.Column<string>(nullable: true),
                    Authorised = table.Column<bool>(nullable: false),
                    ValidUntil = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyReleaseWorkflowState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryExposureKeys",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    OwnerId = table.Column<long>(nullable: false),
                    KeyData = table.Column<byte[]>(nullable: false),
                    RollingStartNumber = table.Column<int>(nullable: false),
                    RollingPeriod = table.Column<int>(nullable: false),
                    TransmissionRiskLevel = table.Column<int>(nullable: false),
                    Region = table.Column<string>(nullable: false),
                    PublishingState = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryExposureKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemporaryExposureKeys_KeyReleaseWorkflowState_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "KeyReleaseWorkflowState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryExposureKeys_OwnerId",
                table: "TemporaryExposureKeys",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemporaryExposureKeys");

            migrationBuilder.DropTable(
                name: "KeyReleaseWorkflowState");

            migrationBuilder.DropSequence(
                name: "EntityFrameworkHiLoSequence");
        }
    }
}
