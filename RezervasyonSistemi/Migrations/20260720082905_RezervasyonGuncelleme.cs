using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RezervasyonSistemi.Migrations
{
    /// <inheritdoc />
    public partial class RezervasyonGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Durum",
                table: "Rezervasyonlar",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Rezervasyonlar",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "KaynakId",
                table: "Rezervasyonlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_KaynakId",
                table: "Rezervasyonlar",
                column: "KaynakId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervasyonlar_Kaynaklar_KaynakId",
                table: "Rezervasyonlar",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervasyonlar_Kaynaklar_KaynakId",
                table: "Rezervasyonlar");

            migrationBuilder.DropIndex(
                name: "IX_Rezervasyonlar_KaynakId",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "KaynakId",
                table: "Rezervasyonlar");

            migrationBuilder.AlterColumn<string>(
                name: "Durum",
                table: "Rezervasyonlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Rezervasyonlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
