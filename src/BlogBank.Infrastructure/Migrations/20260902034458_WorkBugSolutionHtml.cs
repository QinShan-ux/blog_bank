using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkBugSolutionHtml : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Solution",
                table: "work_bugs",
                type: "longtext",
                nullable: false,
                comment: "处理方法（HTML 格式），修复方案或临时绕过方式，可含代码块、加粗、列表等标签",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComment: "处理方法，修复方案或临时绕过方式")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Solution",
                table: "work_bugs",
                type: "longtext",
                nullable: false,
                comment: "处理方法，修复方案或临时绕过方式",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldComment: "处理方法（HTML 格式），修复方案或临时绕过方式，可含代码块、加粗、列表等标签")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
