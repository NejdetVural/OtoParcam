using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtoParcam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcquisitionBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcquisitionBatchId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcquisitionBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcquisitionBatches", x => x.Id);
                    table.CheckConstraint("CK_AcquisitionBatch_TotalCost", "[TotalCost] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_AcquisitionBatchId",
                table: "Products",
                column: "AcquisitionBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AcquisitionBatches_AcquisitionBatchId",
                table: "Products",
                column: "AcquisitionBatchId",
                principalTable: "AcquisitionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AcquisitionBatches_AcquisitionBatchId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "AcquisitionBatches");

            migrationBuilder.DropIndex(
                name: "IX_Products_AcquisitionBatchId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AcquisitionBatchId",
                table: "Products");
        }
    }
}
