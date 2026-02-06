using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShopApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOptionGroupsToTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionGroups_Products_ProductId",
                table: "OptionGroups");

            migrationBuilder.DropIndex(
                name: "IX_OptionGroups_ProductId",
                table: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "FatherId",
                table: "OptionItems");

            migrationBuilder.DropColumn(
                name: "FatherId",
                table: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OptionGroups");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAdjustment",
                table: "OptionItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");

            migrationBuilder.AddColumn<int>(
                name: "DependsOnOptionItemId",
                table: "OptionGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OptionGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductOptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OptionGroupId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOptionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOptionGroups_OptionGroups_OptionGroupId",
                        column: x => x.OptionGroupId,
                        principalTable: "OptionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOptionGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionGroups_Name",
                table: "OptionGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOptionGroups_OptionGroupId",
                table: "ProductOptionGroups",
                column: "OptionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOptionGroups_ProductId",
                table: "ProductOptionGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOptionGroups_ProductId_OptionGroupId",
                table: "ProductOptionGroups",
                columns: new[] { "ProductId", "OptionGroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductOptionGroups");

            migrationBuilder.DropIndex(
                name: "IX_OptionGroups_Name",
                table: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "DependsOnOptionItemId",
                table: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "OptionGroups");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAdjustment",
                table: "OptionItems",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "FatherId",
                table: "OptionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FatherId",
                table: "OptionGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "OptionGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OptionGroups_ProductId",
                table: "OptionGroups",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionGroups_Products_ProductId",
                table: "OptionGroups",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
