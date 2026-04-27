using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchOptimizationBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Field = table.Column<byte>(type: "tinyint", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTokens_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTokens_DocumentId",
                table: "DocumentTokens",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTokens_Token_Lookup",
                table: "DocumentTokens",
                columns: new[] { "Token", "DocumentId", "Field", "Frequency" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentTokens");
        }
    }
}
