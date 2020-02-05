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
                    channel_id = table.Column<Guid>(nullable: false),
                    channel_username = table.Column<string>(nullable: false),
                    title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.channel_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(nullable: false),
                    nickname = table.Column<string>(nullable: false),
                    email = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.UniqueConstraint("AK_users_email", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "emails",
                columns: table => new
                {
                    msg_id = table.Column<Guid>(nullable: false),
                    ChannelId = table.Column<Guid>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    sender_email = table.Column<string>(nullable: false),
                    sender_nickname = table.Column<string>(nullable: false),
                    title = table.Column<string>(nullable: false),
                    body_plain = table.Column<string>(nullable: false),
                    body_html = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emails", x => x.msg_id);
                    table.ForeignKey(
                        name: "FK_emails_channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_user_table",
                columns: table => new
                {
                    user_id = table.Column<Guid>(nullable: false),
                    channel_id = table.Column<Guid>(nullable: false),
                    can_send_message = table.Column<bool>(nullable: false),
                    can_receive_message = table.Column<bool>(nullable: false),
                    can_edit_roles = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_user_table", x => new { x.user_id, x.channel_id });
                    table.ForeignKey(
                        name: "FK_channel_user_table_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_channel_user_table_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    attachment_id = table.Column<Guid>(nullable: false),
                    filename = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false),
                    content_type = table.Column<string>(nullable: false),
                    size = table.Column<long>(nullable: false),
                    HatoMessageMsgId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.attachment_id);
                    table.ForeignKey(
                        name: "FK_attachments_emails_HatoMessageMsgId",
                        column: x => x.HatoMessageMsgId,
                        principalTable: "emails",
                        principalColumn: "msg_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_attachment_id",
                table: "attachments",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_HatoMessageMsgId",
                table: "attachments",
                column: "HatoMessageMsgId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_channel_id",
                table: "channel_user_table",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_user_id",
                table: "channel_user_table",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_channel_user_table_user_id_channel_id",
                table: "channel_user_table",
                columns: new[] { "user_id", "channel_id" });

            migrationBuilder.CreateIndex(
                name: "IX_channels_channel_id",
                table: "channels",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_channels_channel_username",
                table: "channels",
                column: "channel_username");

            migrationBuilder.CreateIndex(
                name: "IX_emails_ChannelId",
                table: "emails",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_id",
                table: "users",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "channel_user_table");

            migrationBuilder.DropTable(
                name: "emails");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
