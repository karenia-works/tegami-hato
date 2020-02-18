using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class AddProperAttachmentRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_messages_hato_message_msg_id",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "ix_attachments_hato_message_msg_id",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "hato_message_msg_id",
                table: "attachments");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "messages",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: "attachments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "attachment_relations",
                columns: table => new
                {
                    attachment_id = table.Column<Guid>(nullable: false),
                    msg_id = table.Column<Guid>(nullable: false),
                    rel_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachment_relations", x => new { x.attachment_id, x.msg_id });
                    table.ForeignKey(
                        name: "fk_attachment_relations_attachments_attachment_id",
                        column: x => x.attachment_id,
                        principalTable: "attachments",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attachment_relations_messages_msg_id",
                        column: x => x.msg_id,
                        principalTable: "messages",
                        principalColumn: "msg_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attachment_relations_attachment_id",
                table: "attachment_relations",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachment_relations_msg_id",
                table: "attachment_relations",
                column: "msg_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachment_relations_attachment_id_msg_id",
                table: "attachment_relations",
                columns: new[] { "attachment_id", "msg_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachment_relations");

            migrationBuilder.DropColumn(
                name: "is_available",
                table: "attachments");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "hato_message_msg_id",
                table: "attachments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_attachments_hato_message_msg_id",
                table: "attachments",
                column: "hato_message_msg_id");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_messages_hato_message_msg_id",
                table: "attachments",
                column: "hato_message_msg_id",
                principalTable: "messages",
                principalColumn: "msg_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
