using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeShopApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductDetailsAddOptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Products",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "OptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    AllowMultiple = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionGroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PriceAdjustment = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionItems_OptionGroups_OptionGroupId",
                        column: x => x.OptionGroupId,
                        principalTable: "OptionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionGroups_ProductId",
                table: "OptionGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionItems_OptionGroupId",
                table: "OptionItems",
                column: "OptionGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionItems");

            migrationBuilder.DropTable(
                name: "OptionGroups");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Cà phê Robusta đậm đặc hương vị truyền thống", null, "Cà phê đen đá" },
                    { 2, 1, "Sữa nóng pha thêm chút cà phê, ngọt ngào", null, "Bạc xỉu" },
                    { 3, 3, "Bánh sừng bò ngàn lớp thơm bơ", null, "Bánh Croissant" },
                    { 4, 1, "Hương vị cà phê Việt Nam truyền thống kết hợp sữa đặc", null, "Cà phê sữa đá" },
                    { 5, 1, "Espresso nhẹ nhàng hòa quyện với lớp sữa tươi đánh nóng", null, "Latte Nóng" },
                    { 6, 1, "Sự cân bằng hoàn hảo giữa Espresso, sữa nóng và bọt sữa", null, "Cappuccino" },
                    { 7, 1, "Espresso pha loãng với nước và đá, thanh nhẹ", null, "Americano Đá" },
                    { 8, 1, "Sữa tươi, vani, Espresso và sốt Caramel ngọt ngào", null, "Caramel Macchiato" },
                    { 9, 2, "Trà đen thơm lừng kết hợp đào ngâm và sả tươi", null, "Trà Đào Cam Sả" },
                    { 10, 2, "Trà ô long thanh mát kết hợp hạt sen bùi bùi", null, "Trà Sen Vàng" },
                    { 11, 2, "Vị ngọt ngào của vải thiều hòa quyện hương hoa hồng", null, "Trà Vải Hoa Hồng" },
                    { 12, 4, "Bột trà xanh Nhật Bản xay nhuyễn với sữa tươi", null, "Matcha Đá Xay" },
                    { 13, 4, "Bánh Oreo xay cùng sữa và đá, phủ lớp kem tươi", null, "Cookie Đá Xay" },
                    { 14, 3, "Bánh ngọt vị cà phê và rượu rum, lớp kem phô mai", null, "Tiramisu" },
                    { 15, 3, "Bánh phô mai chua ngọt vị chanh dây", null, "Cheesecake Chanh Dây" },
                    { 16, 3, "Bánh mousse mềm mịn đậm đà vị sô cô la", null, "Mousse Chocolate" },
                    { 17, 3, "Bánh mì que giòn rụm với nhân pate cay nồng", null, "Bánh Mì Que Pate" },
                    { 18, 3, "Bánh vòng chiên phủ lớp sốt socola đen", null, "Bánh Donut Socola" }
                });

            migrationBuilder.InsertData(
                table: "ProductDetails",
                columns: new[] { "Id", "Price", "ProductId", "Size" },
                values: new object[,]
                {
                    { 1, 20000m, 1, "S" },
                    { 2, 25000m, 1, "M" },
                    { 3, 30000m, 1, "L" },
                    { 4, 28000m, 2, "S" },
                    { 5, 35000m, 2, "M" },
                    { 6, 45000m, 3, "M" },
                    { 7, 35000m, 3, "S" },
                    { 8, 29000m, 4, "M" },
                    { 9, 35000m, 4, "L" },
                    { 10, 45000m, 5, "M" },
                    { 11, 55000m, 5, "L" },
                    { 12, 45000m, 6, "M" },
                    { 13, 55000m, 6, "L" },
                    { 14, 35000m, 7, "M" },
                    { 15, 41000m, 7, "L" },
                    { 16, 50000m, 8, "M" },
                    { 17, 60000m, 8, "L" },
                    { 18, 45000m, 9, "M" },
                    { 19, 55000m, 9, "L" },
                    { 20, 45000m, 10, "M" },
                    { 21, 55000m, 10, "L" },
                    { 22, 42000m, 11, "M" },
                    { 23, 52000m, 11, "L" },
                    { 24, 55000m, 12, "M" },
                    { 25, 65000m, 12, "L" },
                    { 26, 55000m, 13, "M" },
                    { 27, 65000m, 13, "L" },
                    { 28, 39000m, 14, "M" },
                    { 29, 29000m, 14, "S" },
                    { 30, 39000m, 15, "M" },
                    { 31, 29000m, 15, "S" },
                    { 32, 35000m, 16, "M" },
                    { 33, 25000m, 16, "S" },
                    { 34, 15000m, 17, "1 cái" },
                    { 35, 39000m, 17, "Combo 3 cái" },
                    { 36, 25000m, 18, "M" },
                    { 37, 15000m, 18, "S" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_ProductId",
                table: "ProductDetails",
                column: "ProductId");
        }
    }
}
