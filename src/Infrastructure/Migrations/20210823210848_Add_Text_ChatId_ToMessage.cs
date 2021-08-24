using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Add_Text_ChatId_ToMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "EventBase<Message>",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "EventBase<Message>");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "EventBase<Message>");
        }
    }
}
