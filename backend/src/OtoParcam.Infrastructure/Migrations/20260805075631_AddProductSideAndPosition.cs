using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtoParcam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSideAndPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "Products");
        }
    }
}
