using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Items_ItemId",
                table: "Loans");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Loans",
                newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_ItemId",
                table: "Loans",
                newName: "IX_Loans_AssetId");

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.ItemId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Assets_AssetId",
                table: "Loans",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Assets_AssetId",
                table: "Loans");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "Loans",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_AssetId",
                table: "Loans",
                newName: "IX_Loans_ItemId");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Items_ItemId",
                table: "Loans",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
