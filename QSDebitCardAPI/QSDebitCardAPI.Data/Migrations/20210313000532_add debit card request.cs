using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QSDebitCardAPI.Data.Migrations
{
    public partial class adddebitcardrequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

           
            migrationBuilder.CreateTable(
                name: "DEBIT_CARD_DETAILS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CUSTOMER_REQ_ID = table.Column<long>(type: "bigint", nullable: false),
                    ACCOUNT_STATUS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AuthType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BVN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DATE_OF_BIRTH = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    REQUEST_TYPE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NAME_ON_CARD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BRANCH = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ACCOUNT_TO_DEBIT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HOTLISTED_CARD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HOTLIST_CODE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CASE_ID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CURRENT_STEP = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SUBMITTED = table.Column<bool>(type: "bit", maxLength: 100, nullable: false),
                    I_ACCEPT_TERMS_AND_CONDITIONS = table.Column<bool>(type: "bit", maxLength: 100, nullable: false),
                    DATE_OF_ACCEPTING_T_AND_C = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEBIT_CARD_DETAILS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DEBIT_CARD_REQUEST_DETAILS",
                        column: x => x.CUSTOMER_REQ_ID,
                        principalTable: "CUSTOMER_REQUEST",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DEBIT_CARD_DOCS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DEBIT_CARD_REQ_ID = table.Column<int>(type: "int", nullable: false),
                    FILE_NAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TITLE = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContentOrPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOCUMENT_CONTENT_TYPE = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEBIT_CARD_DOCS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DEBIT_CARD_DOC_DEBIT_CARD_REQ_ID",
                        column: x => x.DEBIT_CARD_REQ_ID,
                        principalTable: "DEBIT_CARD_DETAILS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

           

            migrationBuilder.CreateIndex(
                name: "IX_DEBIT_CARD_DETAILS_CUSTOMER_REQ_ID",
                table: "DEBIT_CARD_DETAILS",
                column: "CUSTOMER_REQ_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DEBIT_CARD_DOCS_DEBIT_CARD_REQ_ID",
                table: "DEBIT_CARD_DOCS",
                column: "DEBIT_CARD_REQ_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropTable(
                name: "DEBIT_CARD_DOCS");

            migrationBuilder.DropTable(
                name: "DEBIT_CARD_DETAILS");

          
        }
    }
}
