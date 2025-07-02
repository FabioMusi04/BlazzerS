using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class Cascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies",
                column: "Attachment1Id",
                principalTable: "UploadFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies",
                column: "Attachment1Id",
                principalTable: "UploadFiles",
                principalColumn: "Id");
        }
    }
}
