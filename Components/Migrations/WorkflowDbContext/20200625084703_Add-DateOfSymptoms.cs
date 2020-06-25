using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.WorkflowDbContext
{
    public partial class AddDateOfSymptoms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfSymptomsOnset",
                schema: "dbo",
                table: "KeyReleaseWorkflowState",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfSymptomsOnset",
                schema: "dbo",
                table: "KeyReleaseWorkflowState");
        }
    }
}
