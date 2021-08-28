using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Add_NewMusicFile_And_LoadingProcessMessageSent_Events : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullPath",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "EventBase<Message>",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullPath",
                table: "EventBase<Message>");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "EventBase<Message>");
        }
    }
}
