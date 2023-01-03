using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SAPSalesOrder.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    EmployeeName = table.Column<string>(nullable: true),
                    OrderCode = table.Column<string>(nullable: true),
                    DocNum = table.Column<string>(nullable: true),
                    DocType = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    OrderCode = table.Column<string>(nullable: true),
                    ItemCode = table.Column<string>(nullable: true),
                    UnitPrice = table.Column<string>(nullable: true),
                    Quantity = table.Column<string>(nullable: true),
                    Discount = table.Column<string>(nullable: true),
                    SalesOrderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_SalesOrderId",
                table: "OrderItem",
                column: "SalesOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "SalesOrders");
        }
    }
}
