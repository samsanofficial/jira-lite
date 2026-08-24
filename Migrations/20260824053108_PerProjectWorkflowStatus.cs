using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace jira_lite.Migrations
{
    /// <inheritdoc />
    public partial class PerProjectWorkflowStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "WorkflowStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TreeNodes",
                columns: table => new
                {
                    NodeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NodeId = table.Column<int>(type: "int", nullable: false),
                    EpicId = table.Column<int>(type: "int", nullable: true),
                    StoryId = table.Column<int>(type: "int", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    TreePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoryPoints = table.Column<int>(type: "int", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoggedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.UpdateData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProjectId",
                value: null);

            migrationBuilder.InsertData(
                table: "WorkflowStatuses",
                columns: new[] { "Id", "Color", "Name", "Order", "ProjectId" },
                values: new object[,]
                {
                    { 5, "#e2e8f0", "Todo", 1, 1 },
                    { 6, "#3b82f6", "In Progress", 2, 1 },
                    { 7, "#22c55e", "Done", 3, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_ProjectId",
                table: "WorkflowStatuses",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStatuses_Projects_ProjectId",
                table: "WorkflowStatuses",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStatuses_Projects_ProjectId",
                table: "WorkflowStatuses");

            migrationBuilder.DropTable(
                name: "TreeNodes");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStatuses_ProjectId",
                table: "WorkflowStatuses");

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WorkflowStatuses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "WorkflowStatuses");
        }
    }
}
