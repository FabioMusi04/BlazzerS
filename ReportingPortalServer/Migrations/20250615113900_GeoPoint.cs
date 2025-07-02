using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ReportingPortalServer.Migrations
{
    /// <inheritdoc />
    public partial class GeoPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Reports");

            migrationBuilder.AddColumn<Point>(
                name: "GeoPoint",
                table: "Reports",
                type: "geography",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeoPoint",
                table: "Reports");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Reports",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Reports",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
