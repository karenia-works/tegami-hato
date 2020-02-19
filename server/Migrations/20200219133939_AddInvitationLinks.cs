using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class AddInvitationLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_edit_roles",
                table: "channel_user_table");

            migrationBuilder.DropColumn(
                name: "can_receive_message",
                table: "channel_user_table");

            migrationBuilder.DropColumn(
                name: "can_send_message",
                table: "channel_user_table");

            migrationBuilder.DropColumn(
                name: "rel_id",
                table: "attachment_relations");

            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                table: "users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<List<string>>(
                name: "tags",
                table: "messages",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "channel_username",
                table: "channels",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "default_permission",
                table: "channels",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_creator",
                table: "channel_user_table",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "permission",
                table: "channel_user_table",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "should_receive_message",
                table: "channel_user_table",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "invitation_links",
                columns: table => new
                {
                    link_id = table.Column<string>(nullable: false),
                    channel_id = table.Column<Guid>(nullable: false),
                    default_permission = table.Column<long>(nullable: false),
                    expires = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitation_links", x => x.link_id);
                    table.ForeignKey(
                        name: "fk_invitation_links_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invitation_links_channel_id",
                table: "invitation_links",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitation_links_link_id",
                table: "invitation_links",
                column: "link_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitation_links");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "default_permission",
                table: "channels");

            migrationBuilder.DropColumn(
                name: "is_creator",
                table: "channel_user_table");

            migrationBuilder.DropColumn(
                name: "permission",
                table: "channel_user_table");

            migrationBuilder.DropColumn(
                name: "should_receive_message",
                table: "channel_user_table");

            migrationBuilder.AlterColumn<string>(
                name: "nickname",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "channel_username",
                table: "channels",
                type: "text",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<bool>(
                name: "can_edit_roles",
                table: "channel_user_table",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_receive_message",
                table: "channel_user_table",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_send_message",
                table: "channel_user_table",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "rel_id",
                table: "attachment_relations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
