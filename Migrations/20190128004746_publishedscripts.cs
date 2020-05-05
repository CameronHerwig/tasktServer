using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace tasktServer.Migrations
{
    public partial class PublishedScripts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishedScripts",
                columns: table => new
                {
                    PublishedScriptID = table.Column<Guid>(nullable: false),
                    WorkerID = table.Column<Guid>(nullable: false),
                    PublishedOn = table.Column<DateTime>(nullable: false),
                    ScriptType = table.Column<int>(nullable: false),
                    ScriptData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedScripts", x => x.PublishedScriptID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublishedScripts");
        }
    }
}
