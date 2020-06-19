using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.ExposureContentDbContext
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "EntityFrameworkHiLoSequence",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "AppConfigContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    PublishingId = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    ContentTypeName = table.Column<string>(nullable: true),
                    SignedContent = table.Column<byte[]>(nullable: true),
                    SignedContentTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EksCreateJobInput",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Used = table.Column<bool>(nullable: false),
                    KeyData = table.Column<byte[]>(nullable: false),
                    RollingStartNumber = table.Column<int>(nullable: false),
                    RollingPeriod = table.Column<int>(nullable: false),
                    TransmissionRiskLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EksCreateJobInput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EksCreateJobOutput",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    Region = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    CreatingJobName = table.Column<string>(nullable: false),
                    CreatingJobQualifier = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EksCreateJobOutput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExposureKeySetContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    PublishingId = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    ContentTypeName = table.Column<string>(nullable: true),
                    SignedContent = table.Column<byte[]>(nullable: true),
                    SignedContentTypeName = table.Column<string>(nullable: true),
                    CreatingJobName = table.Column<string>(nullable: false),
                    CreatingJobQualifier = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExposureKeySetContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manifest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    PublishingId = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    ContentTypeName = table.Column<string>(nullable: true),
                    SignedContent = table.Column<byte[]>(nullable: true),
                    SignedContentTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceBundleContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    PublishingId = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    ContentTypeName = table.Column<string>(nullable: true),
                    SignedContent = table.Column<byte[]>(nullable: true),
                    SignedContentTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceBundleContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskCalculationContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Release = table.Column<DateTime>(nullable: false),
                    PublishingId = table.Column<string>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    ContentTypeName = table.Column<string>(nullable: true),
                    SignedContent = table.Column<byte[]>(nullable: true),
                    SignedContentTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskCalculationContent", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigContent");

            migrationBuilder.DropTable(
                name: "EksCreateJobInput");

            migrationBuilder.DropTable(
                name: "EksCreateJobOutput");

            migrationBuilder.DropTable(
                name: "ExposureKeySetContent");

            migrationBuilder.DropTable(
                name: "Manifest");

            migrationBuilder.DropTable(
                name: "ResourceBundleContent");

            migrationBuilder.DropTable(
                name: "RiskCalculationContent");

            migrationBuilder.DropSequence(
                name: "EntityFrameworkHiLoSequence");
        }
    }
}
