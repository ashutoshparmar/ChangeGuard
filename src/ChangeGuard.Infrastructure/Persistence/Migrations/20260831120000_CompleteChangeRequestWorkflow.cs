using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeGuard.Infrastructure.Persistence.Migrations;

public partial class CompleteChangeRequestWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "ChangeRequests",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "QaEvidenceNotes",
            table: "ChangeRequests",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RollbackPlan",
            table: "ChangeRequests",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "UpdatedUtc",
            table: "ChangeRequests",
            type: "datetimeoffset",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()");

        migrationBuilder.CreateTable(
            name: "ChangeRequestAuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                ChangeRequestId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                Action = table.Column<string>(
                    type: "nvarchar(80)",
                    maxLength: 80,
                    nullable: false),
                Actor = table.Column<string>(
                    type: "nvarchar(200)",
                    maxLength: 200,
                    nullable: false),
                Comment = table.Column<string>(
                    type: "nvarchar(2000)",
                    maxLength: 2000,
                    nullable: false),
                FromStatus = table.Column<string>(
                    type: "nvarchar(40)",
                    maxLength: 40,
                    nullable: true),
                ToStatus = table.Column<string>(
                    type: "nvarchar(40)",
                    maxLength: 40,
                    nullable: false),
                OccurredUtc = table.Column<DateTimeOffset>(
                    type: "datetimeoffset",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_ChangeRequestAuditEntries",
                    x => x.Id);
                table.ForeignKey(
                    name: "FK_ChangeRequestAuditEntries_ChangeRequests_ChangeRequestId",
                    column: x => x.ChangeRequestId,
                    principalTable: "ChangeRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChangeRequestAuditEntries_Request_OccurredUtc",
            table: "ChangeRequestAuditEntries",
            columns: new[] { "ChangeRequestId", "OccurredUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChangeRequestAuditEntries");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "ChangeRequests");

        migrationBuilder.DropColumn(
            name: "QaEvidenceNotes",
            table: "ChangeRequests");

        migrationBuilder.DropColumn(
            name: "RollbackPlan",
            table: "ChangeRequests");

        migrationBuilder.DropColumn(
            name: "UpdatedUtc",
            table: "ChangeRequests");
    }
}
