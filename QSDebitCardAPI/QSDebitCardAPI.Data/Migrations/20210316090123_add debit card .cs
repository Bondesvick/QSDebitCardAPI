using Microsoft.EntityFrameworkCore.Migrations;

namespace QSDebitCardAPI.Data.Migrations
{
    public partial class adddebitcard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "DEBIT_CARD_DETAILS",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardType",
                table: "DEBIT_CARD_DETAILS");
        }
    }
}
