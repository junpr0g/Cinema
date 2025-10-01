using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class updateCinema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cinemas_Halls_HallId",
                table: "Cinemas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cinemas_MovieScreenings_MovieScreeningId",
                table: "Cinemas");

            migrationBuilder.DropIndex(
                name: "IX_Cinemas_HallId",
                table: "Cinemas");

            migrationBuilder.DropIndex(
                name: "IX_Cinemas_MovieScreeningId",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "HallId",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "MoovieScreeningId",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "MovieScreeningId",
                table: "Cinemas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HallId",
                table: "Cinemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MoovieScreeningId",
                table: "Cinemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MovieScreeningId",
                table: "Cinemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cinemas_HallId",
                table: "Cinemas",
                column: "HallId");

            migrationBuilder.CreateIndex(
                name: "IX_Cinemas_MovieScreeningId",
                table: "Cinemas",
                column: "MovieScreeningId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cinemas_Halls_HallId",
                table: "Cinemas",
                column: "HallId",
                principalTable: "Halls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cinemas_MovieScreenings_MovieScreeningId",
                table: "Cinemas",
                column: "MovieScreeningId",
                principalTable: "MovieScreenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
