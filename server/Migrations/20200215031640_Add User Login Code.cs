using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class AddUserLoginCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_channel_user_table_channels_channel_temp_id1",
                table: "channel_user_table");

            migrationBuilder.DropForeignKey(
                name: "fk_channel_user_table_users_user_temp_id",
                table: "channel_user_table");

            migrationBuilder.DropForeignKey(
                name: "fk_messages_channels_channel_temp_id",
                table: "messages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_users_email",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_messages",
                table: "messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channels",
                table: "channels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_channel_user_table",
                table: "channel_user_table");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attachments",
                table: "attachments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_users_UserId",
                table: "users",
                newName: "ix_users_user_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "messages",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "MsgId",
                table: "messages",
                newName: "msg_id");

            migrationBuilder.RenameIndex(
                name: "IX_messages_tsvector",
                table: "messages",
                newName: "ix_messages_tsvector");

            migrationBuilder.RenameIndex(
                name: "IX_messages_MsgId",
                table: "messages",
                newName: "ix_messages_msg_id");

            migrationBuilder.RenameIndex(
                name: "IX_messages_ChannelId",
                table: "messages",
                newName: "ix_messages_channel_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "channels",
                newName: "channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_channels_channel_username",
                table: "channels",
                newName: "ix_channels_channel_username");

            migrationBuilder.RenameIndex(
                name: "IX_channels_ChannelId",
                table: "channels",
                newName: "ix_channels_channel_id");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "channel_user_table",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "channel_user_table",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_channel_user_table_UserId_ChannelId",
                table: "channel_user_table",
                newName: "ix_channel_user_table_user_id_channel_id");

            migrationBuilder.RenameIndex(
                name: "IX_channel_user_table_UserId",
                table: "channel_user_table",
                newName: "ix_channel_user_table_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_channel_user_table_ChannelId",
                table: "channel_user_table",
                newName: "ix_channel_user_table_channel_id");

            migrationBuilder.RenameColumn(
                name: "_HatoMessageMsgId",
                table: "attachments",
                newName: "hato_message_msg_id");

            migrationBuilder.RenameColumn(
                name: "AttachmentId",
                table: "attachments",
                newName: "attachment_id");

            migrationBuilder.RenameIndex(
                name: "IX_attachments__HatoMessageMsgId",
                table: "attachments",
                newName: "ix_attachments_hato_message_msg_id");

            migrationBuilder.RenameIndex(
                name: "IX_attachments_AttachmentId",
                table: "attachments",
                newName: "ix_attachments_attachment_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "hato_message_msg_id",
                table: "attachments",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_users_email",
                table: "users",
                column: "email");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_messages",
                table: "messages",
                column: "msg_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_channels",
                table: "channels",
                column: "channel_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_channel_user_table",
                table: "channel_user_table",
                columns: new[] { "user_id", "channel_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                table: "attachments",
                column: "attachment_id");

            migrationBuilder.CreateTable(
                name: "user_login_code",
                columns: table => new
                {
                    code_id = table.Column<Guid>(nullable: false),
                    code = table.Column<string>(nullable: false),
                    expires = table.Column<DateTimeOffset>(nullable: false),
                    user_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login_code", x => x.code_id);
                    table.ForeignKey(
                        name: "fk_user_login_code_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_code_code",
                table: "user_login_code",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_code_expires",
                table: "user_login_code",
                column: "expires");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_code_user_id",
                table: "user_login_code",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_messages_hato_message_msg_id",
                table: "attachments",
                column: "hato_message_msg_id",
                principalTable: "messages",
                principalColumn: "msg_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_channel_user_table_channels_channel_id",
                table: "channel_user_table",
                column: "channel_id",
                principalTable: "channels",
                principalColumn: "channel_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_channel_user_table_users_user_id",
                table: "channel_user_table",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_messages_channels_channel_id",
                table: "messages",
                column: "channel_id",
                principalTable: "channels",
                principalColumn: "channel_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_attachments_messages_hato_message_msg_id",
                table: "attachments");

            migrationBuilder.DropForeignKey(
                name: "fk_channel_user_table_channels_channel_id",
                table: "channel_user_table");

            migrationBuilder.DropForeignKey(
                name: "fk_channel_user_table_users_user_id",
                table: "channel_user_table");

            migrationBuilder.DropForeignKey(
                name: "fk_messages_channels_channel_id",
                table: "messages");

            migrationBuilder.DropTable(
                name: "user_login_code");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_users_email",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_messages",
                table: "messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_channels",
                table: "channels");

            migrationBuilder.DropPrimaryKey(
                name: "pk_channel_user_table",
                table: "channel_user_table");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                table: "attachments");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "users",
                newName: "IX_users_email");

            migrationBuilder.RenameIndex(
                name: "ix_users_user_id",
                table: "users",
                newName: "IX_users_UserId");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "messages",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "msg_id",
                table: "messages",
                newName: "MsgId");

            migrationBuilder.RenameIndex(
                name: "ix_messages_tsvector",
                table: "messages",
                newName: "IX_messages_tsvector");

            migrationBuilder.RenameIndex(
                name: "ix_messages_msg_id",
                table: "messages",
                newName: "IX_messages_MsgId");

            migrationBuilder.RenameIndex(
                name: "ix_messages_channel_id",
                table: "messages",
                newName: "IX_messages_ChannelId");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "channels",
                newName: "ChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_channels_channel_username",
                table: "channels",
                newName: "IX_channels_channel_username");

            migrationBuilder.RenameIndex(
                name: "ix_channels_channel_id",
                table: "channels",
                newName: "IX_channels_ChannelId");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "channel_user_table",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "channel_user_table",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_channel_user_table_user_id_channel_id",
                table: "channel_user_table",
                newName: "IX_channel_user_table_UserId_ChannelId");

            migrationBuilder.RenameIndex(
                name: "ix_channel_user_table_user_id",
                table: "channel_user_table",
                newName: "IX_channel_user_table_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_channel_user_table_channel_id",
                table: "channel_user_table",
                newName: "IX_channel_user_table_ChannelId");

            migrationBuilder.RenameColumn(
                name: "hato_message_msg_id",
                table: "attachments",
                newName: "_HatoMessageMsgId");

            migrationBuilder.RenameColumn(
                name: "attachment_id",
                table: "attachments",
                newName: "AttachmentId");

            migrationBuilder.RenameIndex(
                name: "ix_attachments_hato_message_msg_id",
                table: "attachments",
                newName: "IX_attachments__HatoMessageMsgId");

            migrationBuilder.RenameIndex(
                name: "ix_attachments_attachment_id",
                table: "attachments",
                newName: "IX_attachments_AttachmentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "_HatoMessageMsgId",
                table: "attachments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_users_email",
                table: "users",
                column: "email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_messages",
                table: "messages",
                column: "MsgId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_channels",
                table: "channels",
                column: "ChannelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_channel_user_table",
                table: "channel_user_table",
                columns: new[] { "UserId", "ChannelId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_attachments",
                table: "attachments",
                column: "AttachmentId");

            migrationBuilder.AddForeignKey(
                name: "fk_attachments_messages_hato_message_temp_id",
                table: "attachments",
                column: "_HatoMessageMsgId",
                principalTable: "messages",
                principalColumn: "MsgId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_channel_user_table_channels_channel_temp_id1",
                table: "channel_user_table",
                column: "ChannelId",
                principalTable: "channels",
                principalColumn: "ChannelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_channel_user_table_users_user_temp_id",
                table: "channel_user_table",
                column: "UserId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_messages_channels_channel_temp_id",
                table: "messages",
                column: "ChannelId",
                principalTable: "channels",
                principalColumn: "ChannelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
