using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class Fixattachment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "IX_attachments_HatoMessageMsgId",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "HatoMessageMsgId",
                table: "attachments");

            migrationBuilder.AddColumn<Guid>(
                name: "_HatoMessageMsgId",
                table: "attachments",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_attachments__HatoMessageMsgId",
                table: "attachments",
                column: "_HatoMessageMsgId");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments",
                column: "_HatoMessageMsgId",
                principalTable: "messages",
                principalColumn: "MsgId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "IX_attachments__HatoMessageMsgId",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "_HatoMessageMsgId",
                table: "attachments");

            migrationBuilder.AddColumn<Guid>(
                name: "HatoMessageMsgId",
                table: "attachments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_attachments_HatoMessageMsgId",
                table: "attachments",
                column: "HatoMessageMsgId");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments",
                column: "HatoMessageMsgId",
                principalTable: "messages",
                principalColumn: "MsgId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
