using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtoParcam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAcquisitionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AcquisitionCost",
                table: "Products",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcquisitionSource",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_AcquisitionCost",
                table: "Products",
                sql: "[AcquisitionCost] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_AcquisitionCost",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AcquisitionCost",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AcquisitionSource",
                table: "Products");
        }
    }
}
