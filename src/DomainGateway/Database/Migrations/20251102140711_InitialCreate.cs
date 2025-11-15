using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    ServiceName = table.Column<string>(type: "TEXT", nullable: false),
                    InstanceId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceVersion = table.Column<string>(type: "TEXT", nullable: true),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    RegistrationTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastSeenTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    MetadataValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => new { x.ServiceName, x.InstanceId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instances_ServiceName",
                table: "Instances",
                column: "ServiceName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Instances");
        }
    }
}
