using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.ExposureContentDbContext
{
    public partial class FieldUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "RiskCalculationContent",
                newName: "RiskCalculationContent",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ResourceBundleContent",
                newName: "ResourceBundleContent",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Manifest",
                newName: "Manifest",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ExposureKeySetContent",
                newName: "ExposureKeySetContent",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EksCreateJobOutput",
                newName: "EksCreateJobOutput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EksCreateJobInput",
                newName: "EksCreateJobInput",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AppConfigContent",
                newName: "AppConfigContent",
                newSchema: "dbo");

            migrationBuilder.RenameSequence(
                name: "EntityFrameworkHiLoSequence",
                newName: "EntityFrameworkHiLoSequence",
                newSchema: "dbo");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                schema: "dbo",
                table: "RiskCalculationContent",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                schema: "dbo",
                table: "ResourceBundleContent",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                schema: "dbo",
                table: "Manifest",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                schema: "dbo",
                table: "ExposureKeySetContent",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CreatingJobQualifier",
                schema: "dbo",
                table: "ExposureKeySetContent",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatingJobName",
                schema: "dbo",
                table: "ExposureKeySetContent",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                schema: "dbo",
                table: "AppConfigContent",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RiskCalculationContent_PublishingId",
                schema: "dbo",
                table: "RiskCalculationContent",
                column: "PublishingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceBundleContent_PublishingId",
                schema: "dbo",
                table: "ResourceBundleContent",
                column: "PublishingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manifest_PublishingId",
                schema: "dbo",
                table: "Manifest",
                column: "PublishingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExposureKeySetContent_PublishingId",
                schema: "dbo",
                table: "ExposureKeySetContent",
                column: "PublishingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigContent_PublishingId",
                schema: "dbo",
                table: "AppConfigContent",
                column: "PublishingId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiskCalculationContent_PublishingId",
                schema: "dbo",
                table: "RiskCalculationContent");

            migrationBuilder.DropIndex(
                name: "IX_ResourceBundleContent_PublishingId",
                schema: "dbo",
                table: "ResourceBundleContent");

            migrationBuilder.DropIndex(
                name: "IX_Manifest_PublishingId",
                schema: "dbo",
                table: "Manifest");

            migrationBuilder.DropIndex(
                name: "IX_ExposureKeySetContent_PublishingId",
                schema: "dbo",
                table: "ExposureKeySetContent");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigContent_PublishingId",
                schema: "dbo",
                table: "AppConfigContent");

            migrationBuilder.RenameTable(
                name: "RiskCalculationContent",
                schema: "dbo",
                newName: "RiskCalculationContent");

            migrationBuilder.RenameTable(
                name: "ResourceBundleContent",
                schema: "dbo",
                newName: "ResourceBundleContent");

            migrationBuilder.RenameTable(
                name: "Manifest",
                schema: "dbo",
                newName: "Manifest");

            migrationBuilder.RenameTable(
                name: "ExposureKeySetContent",
                schema: "dbo",
                newName: "ExposureKeySetContent");

            migrationBuilder.RenameTable(
                name: "EksCreateJobOutput",
                schema: "dbo",
                newName: "EksCreateJobOutput");

            migrationBuilder.RenameTable(
                name: "EksCreateJobInput",
                schema: "dbo",
                newName: "EksCreateJobInput");

            migrationBuilder.RenameTable(
                name: "AppConfigContent",
                schema: "dbo",
                newName: "AppConfigContent");

            migrationBuilder.RenameSequence(
                name: "EntityFrameworkHiLoSequence",
                schema: "dbo",
                newName: "EntityFrameworkHiLoSequence");

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                table: "RiskCalculationContent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                table: "ResourceBundleContent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                table: "Manifest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                table: "ExposureKeySetContent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<int>(
                name: "CreatingJobQualifier",
                table: "ExposureKeySetContent",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatingJobName",
                table: "ExposureKeySetContent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublishingId",
                table: "AppConfigContent",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);
        }
    }
}
