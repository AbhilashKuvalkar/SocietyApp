using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberSalutation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Salutation",
                table: "Societies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Salutation",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salutation",
                table: "Societies");

            migrationBuilder.DropColumn(
                name: "Salutation",
                table: "Members");
        }
    }
}
