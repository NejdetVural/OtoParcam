using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtoParcam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSoldPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SoldPrice",
                table: "Products",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_SoldPrice",
                table: "Products",
                sql: "[SoldPrice] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_SoldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SoldPrice",
                table: "Products");
        }
    }
}
