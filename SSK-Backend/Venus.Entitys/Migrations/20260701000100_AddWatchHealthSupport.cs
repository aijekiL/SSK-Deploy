using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venus.Entitys.Migrations
{
    public partial class AddWatchHealthSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WatchImei",
                table: "SM_StaffInfo",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SM_WatchHealthReport",
                columns: table => new
                {
                    ObjectId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WatchImei = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StaffId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    Spo2 = table.Column<int>(type: "int", nullable: true),
                    Systolic = table.Column<int>(type: "int", nullable: true),
                    Diastolic = table.Column<int>(type: "int", nullable: true),
                    ReportTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RawHex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifyID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ModifyDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeleteId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SM_WatchHealthReport", x => x.ObjectId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SM_StaffInfo_WatchImei",
                table: "SM_StaffInfo",
                column: "WatchImei");

            migrationBuilder.CreateIndex(
                name: "IX_SM_WatchHealthReport_WatchImei_ReportTime",
                table: "SM_WatchHealthReport",
                columns: new[] { "WatchImei", "ReportTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SM_WatchHealthReport_StaffId_ReportTime",
                table: "SM_WatchHealthReport",
                columns: new[] { "StaffId", "ReportTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SM_WatchHealthReport");

            migrationBuilder.DropIndex(
                name: "IX_SM_StaffInfo_WatchImei",
                table: "SM_StaffInfo");

            migrationBuilder.DropColumn(
                name: "WatchImei",
                table: "SM_StaffInfo");
        }
    }
}
