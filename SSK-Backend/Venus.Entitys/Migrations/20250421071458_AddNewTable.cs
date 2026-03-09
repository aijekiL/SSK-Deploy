using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venus.Entitys.Migrations
{
    public partial class AddNewTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SM_ServiceConfig");

            migrationBuilder.DropTable(
                name: "SM_ServiceLogs");

            migrationBuilder.CreateTable(
                name: "SM_ProjectInfo",
                columns: table => new
                {
                    ObjectId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProjectName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartWorkDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_SM_ProjectInfo", x => x.ObjectId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SM_ProjectInfo");

            migrationBuilder.CreateTable(
                name: "SM_ServiceConfig",
                columns: table => new
                {
                    ObjectID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeleteId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IntervalTime = table.Column<int>(type: "int", nullable: false),
                    LastRunDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ModifyID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    RequestMethod = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunGroup = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ServiceName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceState = table.Column<int>(type: "int", nullable: false),
                    ServiceUrl = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuccessStatusCode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SM_ServiceConfig", x => x.ObjectID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SM_ServiceLogs",
                columns: table => new
                {
                    ObjectID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConfigID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RequestTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Result = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SM_ServiceLogs", x => x.ObjectID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
