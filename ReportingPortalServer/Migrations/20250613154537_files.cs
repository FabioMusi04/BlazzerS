using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class files : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentIds",
                table: "ReportReply",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "Report",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UploadFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Format = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentIds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadFiles_ReportReply_AttachmentIds",
                        column: x => x.AttachmentIds,
                        principalTable: "ReportReply",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Report_FileId",
                table: "Report",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadFiles_AttachmentIds",
                table: "UploadFiles",
                column: "AttachmentIds");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_UploadFiles_FileId",
                table: "Report",
                column: "FileId",
                principalTable: "UploadFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Report_UploadFiles_FileId",
                table: "Report");

            migrationBuilder.DropTable(
                name: "UploadFiles");

            migrationBuilder.DropIndex(
                name: "IX_Report_FileId",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "AttachmentIds",
                table: "ReportReply");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Report");
        }
    }
}
