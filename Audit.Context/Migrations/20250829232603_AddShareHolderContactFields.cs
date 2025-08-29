using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddShareHolderContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cnic",
                table: "ShareHolders",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "ShareHolders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ShareHolders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cnic",
                table: "ShareHolders");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "ShareHolders");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ShareHolders");
        }
    }
}
