using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Venus.Entitys.Migrations
{
    [DbContext(typeof(VenusDbContext))]
    [Migration("20260722000100_AddEmployeeProfileFaceSync")]
    public partial class AddEmployeeProfileFaceSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeProfileId",
                table: "SM_StaffInfo",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "FaceImageId",
                table: "SM_EmployeeProfile",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FaceSyncMessage",
                table: "SM_EmployeeProfile",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FaceSyncStatus",
                table: "SM_EmployeeProfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SM_StaffInfo_EmployeeProfileId",
                table: "SM_StaffInfo",
                column: "EmployeeProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SM_StaffInfo_EmployeeProfileId",
                table: "SM_StaffInfo");

            migrationBuilder.DropColumn(
                name: "EmployeeProfileId",
                table: "SM_StaffInfo");

            migrationBuilder.DropColumn(
                name: "FaceImageId",
                table: "SM_EmployeeProfile");

            migrationBuilder.DropColumn(
                name: "FaceSyncMessage",
                table: "SM_EmployeeProfile");

            migrationBuilder.DropColumn(
                name: "FaceSyncStatus",
                table: "SM_EmployeeProfile");
        }
    }
}
