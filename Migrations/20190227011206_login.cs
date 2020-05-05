using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace tasktServer.Migrations
{
    public partial class Login : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginProfiles",
                columns: table => new
                {
                    LoginID = table.Column<Guid>(nullable: false),
                    LoginName = table.Column<string>(nullable: true),
                    LoginPassword = table.Column<string>(nullable: true),
                    LastSuccessfulLogin = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginProfiles", x => x.LoginID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginProfiles");
        }
    }
}
