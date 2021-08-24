using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventBase<Message>",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    AggregateId = table.Column<long>(type: "INTEGER", nullable: false),
                    event_type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBase<Message>", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventBase<Message>_AggregateId",
                table: "EventBase<Message>",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBase<Message>_event_type",
                table: "EventBase<Message>",
                column: "event_type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventBase<Message>");
        }
    }
}
