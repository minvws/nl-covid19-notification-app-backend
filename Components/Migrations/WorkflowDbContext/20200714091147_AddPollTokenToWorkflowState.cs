using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.WorkflowDbContext
{
    public partial class AddPollTokenToWorkflowState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PollToken",
                schema: "dbo",
                table: "KeyReleaseWorkflowState",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PollToken",
                schema: "dbo",
                table: "KeyReleaseWorkflowState");
        }
    }
}
