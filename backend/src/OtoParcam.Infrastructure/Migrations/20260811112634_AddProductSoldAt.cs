using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtoParcam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSoldAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SoldAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SoldAt",
                table: "Products",
                column: "SoldAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SoldAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SoldAt",
                table: "Products");
        }
    }
}
