using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    ChannelId = table.Column<Guid>(nullable: false),
                    channel_username = table.Column<string>(nullable: true),
                    channel_title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    nickname = table.Column<string>(nullable: false),
                    email = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UserId);
                    table.UniqueConstraint("AK_users_email", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    MsgId = table.Column<Guid>(nullable: false),
                    ChannelId = table.Column<Guid>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    sender_email = table.Column<string>(nullable: false),
                    sender_nickname = table.Column<string>(nullable: true),
                    title = table.Column<string>(nullable: false),
                    body_plain = table.Column<string>(nullable: false),
                    body_html = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.MsgId);
                    table.ForeignKey(
                        name: "fk_messages_channels_channel_temp_id",
                        column: x => x.ChannelId,
                        principalTable: "channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_user_table",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    ChannelId = table.Column<Guid>(nullable: false),
                    can_send_message = table.Column<bool>(nullable: false),
                    can_receive_message = table.Column<bool>(nullable: false),
                    can_edit_roles = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_user_table", x => new { x.UserId, x.ChannelId });
                    table.ForeignKey(
                        name: "fk_channel_user_table_channels_channel_temp_id1",
                        column: x => x.ChannelId,
                        principalTable: "channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_user_table_users_user_temp_id",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(nullable: false),
                    filename = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false),
                    content_type = table.Column<string>(nullable: false),
                    size = table.Column<long>(nullable: false),
                    HatoMessageMsgId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "fk_attachments_messages_hato_message_temp_id",
                        column: x => x.HatoMessageMsgId,
                        principalTable: "messages",
                        principalColumn: "MsgId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_AttachmentId",
                table: "attachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_HatoMessageMsgId",
                table: "attachments",
                column: "HatoMessageMsgId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_ChannelId",
                table: "channel_user_table",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_UserId",
                table: "channel_user_table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_UserId_ChannelId",
                table: "channel_user_table",
                columns: new[] { "UserId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_channels_ChannelId",
                table: "channels",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_channels_channel_username",
                table: "channels",
                column: "channel_username");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ChannelId",
                table: "messages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_UserId",
                table: "users",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "channel_user_table");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
