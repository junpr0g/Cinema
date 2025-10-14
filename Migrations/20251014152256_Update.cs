using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Places");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "Reserveds",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "Reserveds");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Places",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
