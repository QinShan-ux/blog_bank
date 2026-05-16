using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertUserInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "users",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "地址")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Birthday",
                table: "users",
                type: "date",
                nullable: true,
                comment: "生日");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "users");
        }
    }
}
