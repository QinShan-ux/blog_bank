using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "work_bugs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "bug 标题，简明概括问题")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, comment: "bug 现象描述，包含报错信息、复现步骤等")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RootCause = table.Column<string>(type: "longtext", nullable: true, comment: "原因分析，排查后定位到的根本原因；未定位时可为空")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Solution = table.Column<string>(type: "longtext", nullable: false, comment: "处理方法，修复方案或临时绕过方式")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<int>(type: "int", nullable: false, comment: "严重程度：0=低 1=中 2=高"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "处理状态：0=待处理 1=处理中 2=已解决"),
                    Project = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "所属项目/模块，例如：订单系统")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Environment = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "环境，自由文本，例如：生产环境、客户环境 v2.3")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "发生日期"),
                    SolvedDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "解决日期，未解决时为空"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)", comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)", comment: "修改时间"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_bugs", x => x.Id);
                },
                comment: "工作 Bug 记录")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_bugs");
        }
    }
}
