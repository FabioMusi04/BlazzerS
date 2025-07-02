using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class FixReportReplyUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadFiles_ReportReplies_AttachmentIds",
                table: "UploadFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadFiles_AttachmentIds",
                table: "UploadFiles");

            migrationBuilder.DropColumn(
                name: "AttachmentIds",
                table: "UploadFiles");

            migrationBuilder.DropColumn(
                name: "AttachmentIds",
                table: "ReportReplies");

            migrationBuilder.AddColumn<int>(
                name: "Attachment1Id",
                table: "ReportReplies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Attachment2Id",
                table: "ReportReplies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Attachment3Id",
                table: "ReportReplies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportReplies_Attachment1Id",
                table: "ReportReplies",
                column: "Attachment1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReportReplies_Attachment2Id",
                table: "ReportReplies",
                column: "Attachment2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReportReplies_Attachment3Id",
                table: "ReportReplies",
                column: "Attachment3Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies",
                column: "Attachment1Id",
                principalTable: "UploadFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment2Id",
                table: "ReportReplies",
                column: "Attachment2Id",
                principalTable: "UploadFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment3Id",
                table: "ReportReplies",
                column: "Attachment3Id",
                principalTable: "UploadFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment1Id",
                table: "ReportReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment2Id",
                table: "ReportReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_UploadFiles_Attachment3Id",
                table: "ReportReplies");

            migrationBuilder.DropIndex(
                name: "IX_ReportReplies_Attachment1Id",
                table: "ReportReplies");

            migrationBuilder.DropIndex(
                name: "IX_ReportReplies_Attachment2Id",
                table: "ReportReplies");

            migrationBuilder.DropIndex(
                name: "IX_ReportReplies_Attachment3Id",
                table: "ReportReplies");

            migrationBuilder.DropColumn(
                name: "Attachment1Id",
                table: "ReportReplies");

            migrationBuilder.DropColumn(
                name: "Attachment2Id",
                table: "ReportReplies");

            migrationBuilder.DropColumn(
                name: "Attachment3Id",
                table: "ReportReplies");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentIds",
                table: "UploadFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentIds",
                table: "ReportReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadFiles_AttachmentIds",
                table: "UploadFiles",
                column: "AttachmentIds");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadFiles_ReportReplies_AttachmentIds",
                table: "UploadFiles",
                column: "AttachmentIds",
                principalTable: "ReportReplies",
                principalColumn: "Id");
        }
    }
}
