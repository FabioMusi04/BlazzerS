using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Report_Categories_CategoryId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_UploadFiles_FileId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_Users_UserId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportReply_Report_ReportId",
                table: "ReportReply");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Report",
                table: "Report");

            migrationBuilder.RenameTable(
                name: "Report",
                newName: "Reports");

            migrationBuilder.RenameIndex(
                name: "IX_Report_UserId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Report_FileId",
                table: "Reports",
                newName: "IX_Reports_FileId");

            migrationBuilder.RenameIndex(
                name: "IX_Report_CategoryId",
                table: "Reports",
                newName: "IX_Reports_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReply_Reports_ReportId",
                table: "ReportReply",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Categories_CategoryId",
                table: "Reports",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_UploadFiles_FileId",
                table: "Reports",
                column: "FileId",
                principalTable: "UploadFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportReply_Reports_ReportId",
                table: "ReportReply");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Categories_CategoryId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_UploadFiles_FileId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "Report");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId",
                table: "Report",
                newName: "IX_Report_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_FileId",
                table: "Report",
                newName: "IX_Report_FileId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_CategoryId",
                table: "Report",
                newName: "IX_Report_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Report",
                table: "Report",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Categories_CategoryId",
                table: "Report",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_UploadFiles_FileId",
                table: "Report",
                column: "FileId",
                principalTable: "UploadFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Users_UserId",
                table: "Report",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportReply_Report_ReportId",
                table: "ReportReply",
                column: "ReportId",
                principalTable: "Report",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
