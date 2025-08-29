using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Context.Migrations
{
    /// <inheritdoc />
    public partial class MakeShareHoldersStandalone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShareHolders_Clients_ClientId",
                table: "ShareHolders");

            migrationBuilder.DropIndex(
                name: "IX_ShareHolders_ClientId",
                table: "ShareHolders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ShareHolders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "ShareHolders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ShareHolders_ClientId",
                table: "ShareHolders",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShareHolders_Clients_ClientId",
                table: "ShareHolders",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
