using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "articles",
                type: "longtext",
                nullable: false,
                comment: "文章正文（HTML 或 Markdown 格式，由 ContentType 决定）",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComment: "文章正文（HTML 格式）")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "articles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "html",
                comment: "正文内容类型：html / markdown")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "articles");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "articles",
                type: "longtext",
                nullable: false,
                comment: "文章正文（HTML 格式）",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComment: "文章正文（HTML 或 Markdown 格式，由 ContentType 决定）")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
