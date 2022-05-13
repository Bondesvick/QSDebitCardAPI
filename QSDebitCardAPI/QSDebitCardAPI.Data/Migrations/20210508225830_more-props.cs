using Microsoft.EntityFrameworkCore.Migrations;

namespace QSDebitCardAPI.Data.Migrations
{
    public partial class moreprops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CITY",
                table: "DEBIT_CARD_DETAILS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GENDER",
                table: "DEBIT_CARD_DETAILS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MARITAL_STATUS",
                table: "DEBIT_CARD_DETAILS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TITLE",
                table: "DEBIT_CARD_DETAILS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CITY",
                table: "DEBIT_CARD_DETAILS");

            migrationBuilder.DropColumn(
                name: "GENDER",
                table: "DEBIT_CARD_DETAILS");

            migrationBuilder.DropColumn(
                name: "MARITAL_STATUS",
                table: "DEBIT_CARD_DETAILS");

            migrationBuilder.DropColumn(
                name: "TITLE",
                table: "DEBIT_CARD_DETAILS");
        }
    }
}
