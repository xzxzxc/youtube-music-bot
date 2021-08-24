using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Add_ValidationMessage_ToMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValidationMessage",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationMessage",
                table: "EventBase<Message>");
        }
    }
}
