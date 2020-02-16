﻿// <auto-generated />
using System;
using Karenia.TegamiHato.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace Karenia.TegamiHato.Server.Migrations
{
    [DbContext(typeof(EmailSystemContext))]
    [Migration("20200216135455_Add recent message view")]
    partial class Addrecentmessageview
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.ChannelUserRelation", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ChannelId")
                        .HasColumnName("channel_id")
                        .HasColumnType("uuid");

                    b.Property<bool>("CanEditRoles")
                        .HasColumnName("can_edit_roles")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanReceiveMessage")
                        .HasColumnName("can_receive_message")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanSendMessage")
                        .HasColumnName("can_send_message")
                        .HasColumnType("boolean");

                    b.HasKey("UserId", "ChannelId")
                        .HasName("pk_channel_user_table");

                    b.HasIndex("ChannelId")
                        .HasName("ix_channel_user_table_channel_id");

                    b.HasIndex("UserId")
                        .HasName("ix_channel_user_table_user_id");

                    b.HasIndex("UserId", "ChannelId")
                        .HasName("ix_channel_user_table_user_id_channel_id");

                    b.ToTable("channel_user_table");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.HatoAttachment", b =>
                {
                    b.Property<Guid>("AttachmentId")
                        .HasColumnName("attachment_id")
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnName("content_type")
                        .HasColumnType("text");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnName("filename")
                        .HasColumnType("text");

                    b.Property<long>("Size")
                        .HasColumnName("size")
                        .HasColumnType("bigint");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnName("url")
                        .HasColumnType("text");

                    b.Property<Guid?>("_HatoMessageMsgId")
                        .HasColumnName("hato_message_msg_id")
                        .HasColumnType("uuid");

                    b.HasKey("AttachmentId")
                        .HasName("pk_attachments");

                    b.HasIndex("AttachmentId")
                        .HasName("ix_attachments_attachment_id");

                    b.HasIndex("_HatoMessageMsgId")
                        .HasName("ix_attachments_hato_message_msg_id");

                    b.ToTable("attachments");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.HatoChannel", b =>
                {
                    b.Property<Guid>("ChannelId")
                        .HasColumnName("channel_id")
                        .HasColumnType("uuid");

                    b.Property<string>("ChannelTitle")
                        .IsRequired()
                        .HasColumnName("channel_title")
                        .HasColumnType("text");

                    b.Property<string>("ChannelUsername")
                        .HasColumnName("channel_username")
                        .HasColumnType("text");

                    b.Property<bool>("IsPublic")
                        .HasColumnName("is_public")
                        .HasColumnType("boolean");

                    b.HasKey("ChannelId")
                        .HasName("pk_channels");

                    b.HasIndex("ChannelId")
                        .HasName("ix_channels_channel_id");

                    b.HasIndex("ChannelUsername")
                        .HasName("ix_channels_channel_username");

                    b.ToTable("channels");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.HatoMessage", b =>
                {
                    b.Property<Guid>("MsgId")
                        .HasColumnName("msg_id")
                        .HasColumnType("uuid");

                    b.Property<string>("BodyHtml")
                        .HasColumnName("body_html")
                        .HasColumnType("text");

                    b.Property<string>("BodyPlain")
                        .IsRequired()
                        .HasColumnName("body_plain")
                        .HasColumnType("text");

                    b.Property<Guid>("ChannelId")
                        .HasColumnName("channel_id")
                        .HasColumnType("uuid");

                    b.Property<string>("SenderEmail")
                        .IsRequired()
                        .HasColumnName("sender_email")
                        .HasColumnType("text");

                    b.Property<string>("SenderNickname")
                        .HasColumnName("sender_nickname")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("tsvector")
                        .HasColumnName("tsvector")
                        .HasColumnType("tsvector");

                    b.HasKey("MsgId")
                        .HasName("pk_messages");

                    b.HasIndex("ChannelId")
                        .HasName("ix_messages_channel_id");

                    b.HasIndex("MsgId")
                        .HasName("ix_messages_msg_id");

                    b.HasIndex("tsvector")
                        .HasName("ix_messages_tsvector")
                        .HasAnnotation("Npgsql:IndexMethod", "GIN");

                    b.ToTable("messages");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("text");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnName("nickname")
                        .HasColumnType("text");

                    b.HasKey("UserId")
                        .HasName("pk_users");

                    b.HasAlternateKey("Email")
                        .HasName("ak_users_email");

                    b.HasIndex("Email")
                        .HasName("ix_users_email");

                    b.HasIndex("UserId")
                        .HasName("ix_users_user_id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.ChannelUserRelation", b =>
                {
                    b.HasOne("Karenia.TegamiHato.Server.Models.HatoChannel", "_Channel")
                        .WithMany("_Users")
                        .HasForeignKey("ChannelId")
                        .HasConstraintName("fk_channel_user_table_channels_channel_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Karenia.TegamiHato.Server.Models.User", "_User")
                        .WithMany("_Channels")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_channel_user_table_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.HatoAttachment", b =>
                {
                    b.HasOne("Karenia.TegamiHato.Server.Models.HatoMessage", "_HatoMessage")
                        .WithMany("attachments")
                        .HasForeignKey("_HatoMessageMsgId")
                        .HasConstraintName("fk_attachments_messages_hato_message_msg_id");
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.HatoMessage", b =>
                {
                    b.HasOne("Karenia.TegamiHato.Server.Models.HatoChannel", "_Channel")
                        .WithMany("_Messages")
                        .HasForeignKey("ChannelId")
                        .HasConstraintName("fk_messages_channels_channel_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Karenia.TegamiHato.Server.Models.User", b =>
                {
                    b.OwnsMany("Karenia.TegamiHato.Server.Models.UserLoginCode", "_LoginCodes", b1 =>
                        {
                            b1.Property<Guid>("CodeId")
                                .HasColumnName("code_id")
                                .HasColumnType("uuid");

                            b1.Property<string>("Code")
                                .IsRequired()
                                .HasColumnName("code")
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("Expires")
                                .HasColumnName("expires")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<Guid>("UserId")
                                .HasColumnName("user_id")
                                .HasColumnType("uuid");

                            b1.HasKey("CodeId")
                                .HasName("pk_user_login_code");

                            b1.HasIndex("Code")
                                .HasName("ix_user_login_code_code");

                            b1.HasIndex("Expires")
                                .HasName("ix_user_login_code_expires");

                            b1.HasIndex("UserId")
                                .HasName("ix_user_login_code_user_id");

                            b1.ToTable("user_login_code");

                            b1.WithOwner()
                                .HasForeignKey("UserId")
                                .HasConstraintName("fk_user_login_code_users_user_id");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
