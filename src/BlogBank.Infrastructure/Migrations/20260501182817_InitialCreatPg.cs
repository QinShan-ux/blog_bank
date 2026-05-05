using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatPg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "文章标题")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false, comment: "发布日期"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "所属分类，例如：前端开发、JavaScript、DevOps")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadTime = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "预计阅读时长，例如：5 分钟阅读")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, comment: "文章摘要，用于列表页展示")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false, comment: "文章正文（HTML 格式）")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_articles", x => x.Id);
                },
                comment: "博客文章")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TraceId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "操作人ID")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "操作人姓名")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "请求接口地址")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "客户端IP地址")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HttpMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TableName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "操作的数据表名")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "操作的数据记录ID")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, comment: "操作类型：增/删/改/查")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValues = table.Column<string>(type: "longtext", nullable: false, comment: "修改前的值（JSON格式）")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValues = table.Column<string>(type: "longtext", nullable: false, comment: "修改后的值（JSON格式）")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "操作时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                },
                comment: "操作审计日志")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "essays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "随笔标题")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false, comment: "发布日期"),
                    Mood = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "心情文字描述，例如：惬意、专注")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MoodIcon = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, comment: "心情 Emoji 图标，例如：😌")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weather = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "天气文字描述，例如：晴、多云")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeatherIcon = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, comment: "天气 Emoji 图标，例如：☀️")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "写作地点，例如：街角咖啡馆")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, comment: "随笔摘要，用于列表页折叠展示")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false, comment: "随笔正文（纯文本）")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BgColor = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "卡片背景色，十六进制颜色值，例如：#fef3c7")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_essays", x => x.Id);
                },
                comment: "随笔")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Pid = table.Column<long>(type: "bigint", nullable: false, comment: "父菜单 ID，根菜单为 0"),
                    Type = table.Column<int>(type: "int", nullable: false, comment: "菜单类型：1 目录、2 菜单、3 按钮"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, comment: "路由名称，对应前端 Vue Router name 字段")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, comment: "路由地址，对应前端 Vue Router path 字段")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Component = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, comment: "组件文件路径，相对于 views 目录")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Redirect = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, comment: "重定向地址，目录类型使用")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permission = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, comment: "权限标识，格式如 sys:user:add")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "菜单显示名称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, comment: "菜单图标，格式如 ele-Menu")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsIframe = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否以内嵌 iframe 方式打开"),
                    OutLink = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true, comment: "外链 URL")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsHide = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否在侧边栏中隐藏"),
                    IsKeepAlive = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否开启页面缓存（keep-alive）"),
                    IsAffix = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "是否固定在标签栏，固定后不可关闭"),
                    OrderNo = table.Column<int>(type: "int", nullable: false, comment: "排序号，数值越小越靠前"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "菜单状态：0 禁用、1 启用"),
                    Remark = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true, comment: "备注")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_menus", x => x.Id);
                },
                comment: "系统菜单")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "角色编码，程序内部使用，全局唯一，例如：admin、editor")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "角色显示名称，例如：管理员、编辑者")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "角色描述，说明该角色的权限范围")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_roles", x => x.Id);
                },
                comment: "角色")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "用户名，用于登录，全局唯一")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nickname = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "显示昵称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "邮箱地址，全局唯一")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "经过哈希处理的密码")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avatar = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "头像 URL")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "账号是否启用"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)", comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)", comment: "最后更新时间"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                },
                comment: "用户")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "article_tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false, comment: "所属文章 ID（外键）"),
                    Tag = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "标签名称，例如：CSS、响应式")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_article_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_article_tags_articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "文章标签")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "essay_tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    EssayId = table.Column<long>(type: "bigint", nullable: false, comment: "所属随笔 ID（外键）"),
                    Tag = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "标签名称，例如：生活、咖啡")
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    table.PrimaryKey("PK_essay_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_essay_tags_essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "essays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "随笔标签")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_menus",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "关联的用户 ID（联合主键之一）"),
                    MenuId = table.Column<long>(type: "bigint", nullable: false, comment: "关联的菜单 ID（联合主键之一）"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "授权时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_menus", x => new { x.UserId, x.MenuId });
                    table.ForeignKey(
                        name: "FK_user_menus_menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_menus_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "用户菜单关联")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "关联的用户 ID（联合主键之一）"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false, comment: "关联的角色 ID（联合主键之一）"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "授权时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "用户角色关联")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_article_tags_ArticleId",
                table: "article_tags",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_operated_at",
                table: "audit_logs",
                column: "OperatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_user_id",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_essay_tags_EssayId",
                table: "essay_tags",
                column: "EssayId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Code",
                table: "roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_menus_MenuId",
                table: "user_menus",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_tags");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "essay_tags");

            migrationBuilder.DropTable(
                name: "user_menus");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "essays");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
