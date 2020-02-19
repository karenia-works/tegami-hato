using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class SquashedInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("create extension if not exists pg_trgm");
            migrationBuilder.Sql("create extension if not exists pg_jieba");

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    attachment_id = table.Column<Guid>(nullable: false),
                    filename = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false),
                    content_type = table.Column<string>(nullable: false),
                    size = table.Column<long>(nullable: false),
                    is_available = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachments", x => x.attachment_id);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    channel_id = table.Column<Guid>(nullable: false),
                    channel_username = table.Column<string>(nullable: false),
                    channel_title = table.Column<string>(nullable: false),
                    is_public = table.Column<bool>(nullable: false),
                    default_permission = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.channel_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(nullable: false),
                    nickname = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.UniqueConstraint("ak_users_email", x => x.email);
                });

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

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    msg_id = table.Column<Guid>(nullable: false),
                    channel_id = table.Column<Guid>(nullable: false),
                    sender_email = table.Column<string>(nullable: false),
                    sender_nickname = table.Column<string>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    body_plain = table.Column<string>(nullable: false),
                    body_html = table.Column<string>(nullable: true),
                    tsvector = table.Column<NpgsqlTsVector>(nullable: true),
                    tags = table.Column<List<string>>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.msg_id);
                    table.ForeignKey(
                        name: "fk_messages_channels_channel_id",
                        column: x => x.channel_id,
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
                    should_receive_message = table.Column<bool>(nullable: false),
                    is_creator = table.Column<bool>(nullable: false),
                    permission = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_user_table", x => new { x.user_id, x.channel_id });
                    table.ForeignKey(
                        name: "fk_channel_user_table_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_user_table_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "attachment_relations",
                columns: table => new
                {
                    attachment_id = table.Column<Guid>(nullable: false),
                    msg_id = table.Column<Guid>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "ix_attachments_attachment_id",
                table: "attachments",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_user_table_channel_id",
                table: "channel_user_table",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_user_table_user_id",
                table: "channel_user_table",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_user_table_user_id_channel_id",
                table: "channel_user_table",
                columns: new[] { "user_id", "channel_id" });

            migrationBuilder.CreateIndex(
                name: "ix_channels_channel_id",
                table: "channels",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_channel_username",
                table: "channels",
                column: "channel_username");

            migrationBuilder.CreateIndex(
                name: "ix_invitation_links_channel_id",
                table: "invitation_links",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitation_links_link_id",
                table: "invitation_links",
                column: "link_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_channel_id",
                table: "messages",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_msg_id",
                table: "messages",
                column: "msg_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_tsvector",
                table: "messages",
                column: "tsvector")
                .Annotation("Npgsql:IndexMethod", "GIN");

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

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_users_user_id",
                table: "users",
                column: "user_id");


            migrationBuilder.Sql(@"
            CREATE TRIGGER message_search_vector_update BEFORE INSERT OR UPDATE   
            ON messages FOR EACH ROW EXECUTE PROCEDURE
            tsvector_update_trigger(tsvector, 'public.jiebacfg', title, body_plain);");

            migrationBuilder.Sql("UPDATE messages SET title = title;");
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION max(uuid, uuid)
RETURNS uuid AS $$
BEGIN
    IF $1 IS NULL OR $1 < $2 THEN
        RETURN $2;
    END IF;

    RETURN $1;
END;
$$ LANGUAGE plpgsql;


create or replace aggregate max(uuid) (
  sfunc = max,
  stype = uuid,
  combinefunc = max,
  parallel = safe,
  sortop = operator (>)
);            
            ");
            migrationBuilder.Sql(@"
create or replace view recent_messages as
select 
    channels.channel_id,
    channels.channel_title, 
    max_msg.max_msg_id as msg_id,
    messages.title,
    messages.body_plain,
    messages.sender_email,
    messages.sender_nickname
from channels
    join (
        select channel_id, max(msg_id) as max_msg_id from messages
        group by channel_id
    ) as max_msg on channels.channel_id = max_msg.channel_id
    join messages on max_msg.max_msg_id = messages.msg_id
            ");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop trigger if exists message_search_vector_update on messages");
            migrationBuilder.Sql(@"drop view if exists recent_messages");
            migrationBuilder.Sql(@"drop aggregate if exists max(uuid)");
            migrationBuilder.Sql(@"drop function if exists max(uuid,uuid)");

            migrationBuilder.DropTable(
                name: "attachment_relations");

            migrationBuilder.DropTable(
                name: "channel_user_table");

            migrationBuilder.DropTable(
                name: "invitation_links");

            migrationBuilder.DropTable(
                name: "user_login_code");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
