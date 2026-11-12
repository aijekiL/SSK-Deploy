using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venus.Entitys.Migrations
{
    [DbContext(typeof(VenusDbContext))]
    [Migration("20261009000000_AddProjectLedgerRoles")]
    public partial class AddProjectLedgerRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 作业台账新增：专职监护人 / 项目管理员（工程级角色，可编辑）
            migrationBuilder.AddColumn<string>(
                name: "GuardianName",
                table: "sm_projectinfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdministratorName",
                table: "sm_projectinfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "sm_projectinfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardianName",
                table: "sm_projectinfo");

            migrationBuilder.DropColumn(
                name: "AdministratorName",
                table: "sm_projectinfo");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "sm_projectinfo");
        }
    }
}