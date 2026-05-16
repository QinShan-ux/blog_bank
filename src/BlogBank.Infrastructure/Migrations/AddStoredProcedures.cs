using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogBank.Infrastructure.Migrations;

public partial class AddStoredProcedures : Migration
{
    // SQL 文件根目录
    private static readonly string ScriptPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Data", "Scripts", "StoredProcedures");

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 读取并执行所有 SP 文件
        var sqlFiles = Directory.GetFiles(ScriptPath, "*.sql");
        foreach (var file in sqlFiles)
        {
            var sql = File.ReadAllText(file);
            migrationBuilder.Sql(sql);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // 回滚时删除所有 SP
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InsertPatient");
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdatePatient");
    }
}