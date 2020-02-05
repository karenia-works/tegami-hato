using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;
using System.Text.RegularExpressions;

namespace Karenia.TegamiHato.Server.Models
{
    // Disable initialization warning because we don't need that for now
#pragma warning disable CS8618

    public class HatoAttachment
    {
        public Guid AttachmentId { get; set; }
        public string Filename { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }

    public class HatoMessage
    {
        public Guid MsgId { get; set; }

        public HatoChannel Channel { get; set; }

        public DateTime Timestamp { get; set; }

        public string SenderEmail { get; set; }
        public string SenderNickname { get; set; }

        // public List<Address> Receivers { get; set; }

        public string Title { get; set; }

        public string BodyPlain { get; set; }

        public string? BodyHtml { get; set; }

        public ICollection<HatoAttachment> attachments { get; set; }

        private NpgsqlTsVector tsvector { get; set; }
    }

    public class HatoChannel
    {
        public Guid ChannelId { get; set; }

        public string ChannelUsername { get; set; }

        public string Title { get; set; }

        public ICollection<ChannelUserRelation> _Users { get; set; }
        public ICollection<HatoMessage> _Messages { get; set; }
    }

    public class ChannelUserRelation
    {
        public Guid UserId { get; set; }
        public User _User { get; set; }

        public Guid ChannelId { get; set; }
        public HatoChannel _Channel { get; set; }

        public bool CanSendMessage { get; set; }
        public bool CanReceiveMessage { get; set; }
        public bool CanEditRoles { get; set; }
    }

    public class User
    {
        public Guid UserId { get; set; }

        public string Nickname { get; set; }

        public string Email { get; set; }

        public ICollection<ChannelUserRelation> _Channels { get; set; }
    }

    // Disable initialization warning because we don't need that for now

    public class EmailSystemContext : DbContext
    {
        public DbSet<HatoMessage> Emails { get; set; }
        public DbSet<HatoChannel> Channels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChannelUserRelation> ChannelUserTable { get; set; }
        public DbSet<HatoAttachment> Attachments { get; set; }

        public EmailSystemContext(DbContextOptions ctx) : base(ctx)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Replace table names

                entity.SetTableName(entity.GetTableName().ToSnakeCase());

                // Replace column names            
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToSnakeCase());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(index.GetName().ToSnakeCase());
                }
            }

            modelBuilder.Entity<HatoMessage>().HasKey(x => x.MsgId);
            modelBuilder.Entity<HatoMessage>().HasOne(x => x.Channel).WithMany(x => x._Messages);

            modelBuilder.Entity<User>().HasKey(x => x.UserId);
            modelBuilder.Entity<User>().HasIndex(x => x.UserId);
            modelBuilder.Entity<User>().HasAlternateKey(x => x.Email);
            modelBuilder.Entity<User>().HasIndex(x => x.Email);

            modelBuilder.Entity<HatoChannel>().HasKey(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelUsername);

            modelBuilder.Entity<HatoAttachment>().HasKey(x => x.AttachmentId);
            modelBuilder.Entity<HatoAttachment>().HasIndex(x => x.AttachmentId);

            modelBuilder.Entity<ChannelUserRelation>().HasKey(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._User).WithMany(u => u._Channels).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._Channel).WithMany(u => u._Users).HasForeignKey(u => u.ChannelId);
        }
    }


    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
#pragma warning restore CS8618
}
