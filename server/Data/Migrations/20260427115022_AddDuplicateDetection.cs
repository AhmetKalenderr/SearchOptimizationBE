using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchOptimizationBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentContentHashes",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentSha256 = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    NormalizedTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentContentHashes", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_DocumentContentHashes_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentContentHashes_ContentSha256",
                table: "DocumentContentHashes",
                column: "ContentSha256");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentContentHashes_NormalizedTitle",
                table: "DocumentContentHashes",
                column: "NormalizedTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentContentHashes");
        }
    }
}
