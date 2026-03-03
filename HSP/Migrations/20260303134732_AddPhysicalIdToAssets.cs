using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSP.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalIdToAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhysicalId",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhysicalId",
                table: "Assets");
        }
    }
}
