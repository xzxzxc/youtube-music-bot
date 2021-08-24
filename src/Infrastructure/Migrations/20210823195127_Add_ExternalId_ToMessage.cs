using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Add_ExternalId_ToMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalId",
                table: "EventBase<Message>",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "EventBase<Message>");
        }
    }
}
