using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.WorkflowDbContext
{
    public partial class addAuthorisedByCaregiverprop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "TemporaryExposureKeys",
                newName: "TemporaryExposureKeys",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "KeyReleaseWorkflowState",
                newName: "KeyReleaseWorkflowState",
                newSchema: "dbo");

            migrationBuilder.RenameSequence(
                name: "EntityFrameworkHiLoSequence",
                newName: "EntityFrameworkHiLoSequence",
                newSchema: "dbo");

            migrationBuilder.AddColumn<bool>(
                name: "AuthorisedByCaregiver",
                schema: "dbo",
                table: "KeyReleaseWorkflowState",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorisedByCaregiver",
                schema: "dbo",
                table: "KeyReleaseWorkflowState");

            migrationBuilder.RenameTable(
                name: "TemporaryExposureKeys",
                schema: "dbo",
                newName: "TemporaryExposureKeys");

            migrationBuilder.RenameTable(
                name: "KeyReleaseWorkflowState",
                schema: "dbo",
                newName: "KeyReleaseWorkflowState");

            migrationBuilder.RenameSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "dbo",
                newName: "EntityFrameworkHiLoSequence");
        }
    }
}
