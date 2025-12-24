using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeShopApi.Migrations
{
    /// <inheritdoc />
    public partial class addPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
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
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Coffee" },
                    { 2, "Tea" },
                    { 3, "Food" },
                    { 4, "Freeze" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "Module", "Name" },
                values: new object[,]
                {
                    { 1, "product.view", null, "Product", "Xem sản phẩm" },
                    { 2, "product.create", null, "Product", "Tạo sản phẩm" },
                    { 3, "product.update", null, "Product", "Sửa sản phẩm" },
                    { 4, "product.delete", null, "Product", "Xóa sản phẩm" },
                    { 5, "category.view", null, "Category", "Xem danh mục" },
                    { 6, "category.create", null, "Category", "Tạo danh mục" },
                    { 7, "category.update", null, "Category", "Sửa danh mục" },
                    { 8, "category.delete", null, "Category", "Xóa danh mục" },
                    { 9, "order.view.own", null, "Order", "Xem đơn hàng của mình" },
                    { 10, "order.view.all", null, "Order", "Xem tất cả đơn hàng" },
                    { 11, "order.create", null, "Order", "Tạo đơn hàng" },
                    { 12, "order.update.own", null, "Order", "Sửa đơn hàng của mình" },
                    { 13, "order.update.all", null, "Order", "Sửa tất cả đơn hàng" },
                    { 14, "order.cancel.own", null, "Order", "Hủy đơn của mình" },
                    { 15, "order.cancel.all", null, "Order", "Hủy bất kỳ đơn nào" },
                    { 16, "user.view.own", null, "User", "Xem thông tin cá nhân" },
                    { 17, "user.view.all", null, "User", "Xem tất cả user" },
                    { 18, "user.update.own", null, "User", "Sửa thông tin cá nhân" },
                    { 19, "user.update.all", null, "User", "Sửa thông tin user khác" },
                    { 20, "user.delete", null, "User", "Xóa user" },
                    { 21, "role.manage", null, "System", "Quản lý role" },
                    { 22, "permission.assign", null, "System", "Phân quyền" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "ADMIN", "Admin" },
                    { 2, "CUSTOMER", "Khách hàng" },
                    { 3, "STAFF", "Nhân viên" }
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
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 13, 1 },
                    { 14, 1 },
                    { 15, 1 },
                    { 16, 1 },
                    { 17, 1 },
                    { 18, 1 },
                    { 19, 1 },
                    { 20, 1 },
                    { 21, 1 },
                    { 22, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 10, 2 },
                    { 13, 2 },
                    { 16, 2 },
                    { 18, 2 },
                    { 1, 3 },
                    { 5, 3 },
                    { 9, 3 },
                    { 11, 3 },
                    { 14, 3 },
                    { 16, 3 },
                    { 18, 3 }
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
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_ProductId",
                table: "ProductDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
