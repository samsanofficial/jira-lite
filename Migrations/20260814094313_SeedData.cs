using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace jira_lite.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Full access to the project", "Project Admin" },
                    { 2, "Can manage epics and stories", "Lead" },
                    { 3, "Can work on assigned tasks", "Member" },
                    { 4, "Read-only access", "Viewer" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "PasswordHash" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@jiralite.com", "Admin", "$2a$11$ow/8YBLWFpFMJwCBESHpEusFNJOHY8bFJmNrCpRwiqrjrRGRDDfgK" });

            migrationBuilder.InsertData(
                table: "WorkflowStatuses",
                columns: new[] { "Id", "Color", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "#e2e8f0", "Todo", 1 },
                    { 2, "#3b82f6", "In Progress", 2 },
                    { 3, "#f59e0b", "In Review", 3 },
                    { 4, "#22c55e", "Done", 4 }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "CreatedById", "Description", "EndDate", "Key", "Name", "StartDate", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "A lightweight Jira clone for learning purposes", null, "JL", "Jira Lite", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "WorkflowTransitions",
                columns: new[] { "Id", "EntityType", "FromStatusId", "ToStatusId" },
                values: new object[,]
                {
                    { 1, "Epic", 1, 2 },
                    { 2, "Epic", 2, 3 },
                    { 3, "Epic", 3, 4 },
                    { 4, "Epic", 3, 2 },
                    { 5, "Epic", 2, 1 },
                    { 6, "Story", 1, 2 },
                    { 7, "Story", 2, 3 },
                    { 8, "Story", 3, 4 },
                    { 9, "Story", 3, 2 },
                    { 10, "Story", 2, 1 },
                    { 11, "Task", 1, 2 },
                    { 12, "Task", 2, 3 },
                    { 13, "Task", 3, 4 },
                    { 14, "Task", 3, 2 },
                    { 15, "Task", 2, 1 },
                    { 16, "Subtask", 1, 2 },
                    { 17, "Subtask", 2, 3 },
                    { 18, "Subtask", 3, 4 },
                    { 19, "Subtask", 3, 2 },
                    { 20, "Subtask", 2, 1 }
                });

            migrationBuilder.InsertData(
                table: "UserProjectRoles",
                columns: new[] { "ProjectId", "RoleId", "UserId", "AssignedAt" },
                values: new object[] { 1, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UserProjectRoles",
                keyColumns: new[] { "ProjectId", "RoleId", "UserId" },
                keyValues: new object[] { 1, 1, 1 });

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "WorkflowTransitions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
