using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<string>(
            //     name: "Phone",
            //     table: "users",
            //     type: "varchar(20)",
            //     maxLength: 20,
            //     nullable: false,
            //     defaultValue: "",
            //     comment: "电话")
            //     .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "audit_logs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "users");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "audit_logs",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
