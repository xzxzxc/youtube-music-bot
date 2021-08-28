using Microsoft.EntityFrameworkCore.Migrations;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    public partial class Add_FileToBeSentCreated_Event : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullPath",
                table: "EventBase<Message>",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionFilePath",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MusicFilePath",
                table: "EventBase<Message>",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionFilePath",
                table: "EventBase<Message>");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "EventBase<Message>");

            migrationBuilder.DropColumn(
                name: "MusicFilePath",
                table: "EventBase<Message>");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "EventBase<Message>",
                newName: "FullPath");
        }
    }
}
