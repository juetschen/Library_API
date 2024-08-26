using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace LibraryAPI.Migrations
{
    public partial class UpdateBookAndAddLoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Book tablosuna Stock kolonunu ekliyoruz
            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Books",
                nullable: false,
                defaultValue: 0);

            // Loan tablosunu oluşturuyoruz
            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<string>(nullable: false),
                    BookId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<string>(nullable: false),
                    LoanDate = table.Column<DateTime>(nullable: false),
                    ReturnDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_MemberId",
                table: "Loans",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BookId",
                table: "Loans",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_EmployeeId",
                table: "Loans",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Loan tablosunu geri alıyoruz
            migrationBuilder.DropTable(
                name: "Loans");

            // Book tablosundan Stock kolonunu kaldırıyoruz
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Books");
        }
    }
}
