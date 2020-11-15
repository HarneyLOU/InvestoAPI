using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InvestoAPI.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    StockId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: false),
                    IpoDate = table.Column<DateTime>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Exchange = table.Column<string>(nullable: true),
                    Industry = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    MarketCap = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Description = table.Column<string>(nullable: true),
                    MarketIndex = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.StockId);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    StockId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Volume = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => new { x.StockId, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "StocksDaily",
                columns: table => new
                {
                    StockId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Symbol = table.Column<string>(nullable: true),
                    Open = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Volume = table.Column<long>(nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocksDaily", x => new { x.StockId, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: true),
                    PasswordSalt = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "StocksDaily");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
