using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class ReportReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReply_Reports_ReportId",
                table: "ReportReply");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportReply_Users_UserId",
                table: "ReportReply");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadFiles_ReportReply_AttachmentIds",
                table: "UploadFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportReply",
                table: "ReportReply");

            migrationBuilder.RenameTable(
                name: "ReportReply",
                newName: "ReportReplies");

            migrationBuilder.RenameIndex(
                name: "IX_ReportReply_UserId",
                table: "ReportReplies",
                newName: "IX_ReportReplies_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportReply_ReportId",
                table: "ReportReplies",
                newName: "IX_ReportReplies_ReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportReplies",
                table: "ReportReplies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_Reports_ReportId",
                table: "ReportReplies",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReplies_Users_UserId",
                table: "ReportReplies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UploadFiles_ReportReplies_AttachmentIds",
                table: "UploadFiles",
                column: "AttachmentIds",
                principalTable: "ReportReplies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_Reports_ReportId",
                table: "ReportReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportReplies_Users_UserId",
                table: "ReportReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadFiles_ReportReplies_AttachmentIds",
                table: "UploadFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportReplies",
                table: "ReportReplies");

            migrationBuilder.RenameTable(
                name: "ReportReplies",
                newName: "ReportReply");

            migrationBuilder.RenameIndex(
                name: "IX_ReportReplies_UserId",
                table: "ReportReply",
                newName: "IX_ReportReply_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportReplies_ReportId",
                table: "ReportReply",
                newName: "IX_ReportReply_ReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportReply",
                table: "ReportReply",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReply_Reports_ReportId",
                table: "ReportReply",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReply_Users_UserId",
                table: "ReportReply",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UploadFiles_ReportReply_AttachmentIds",
                table: "UploadFiles",
                column: "AttachmentIds",
                principalTable: "ReportReply",
                principalColumn: "Id");
        }
    }
}
