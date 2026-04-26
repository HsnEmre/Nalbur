using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nalbur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class migdeneme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReturnNote",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "Sales",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ReturnNote",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Sales");
        }
    }
}
