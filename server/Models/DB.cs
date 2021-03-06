using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Marques.EFCore.SnakeCase;
using System.Text.Json.Serialization;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karenia.TegamiHato.Server.Models
{
    // Disable initialization warning because we don't need that for now
#pragma warning disable CS8618
#pragma warning disable IDE1006
    [Flags]
    public enum UserPermission : uint
    {
        None = 0,
        Receive = 0x01,
        Send = 0x02,
        Edit = 0x04,
        UserManage = 0x08,
        EditInfo = 0x10,
        FullControl = 0xffffffff,
    }

    public class HatoAttachment
    {
        public Ulid AttachmentId { get; set; }
        public string Filename { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public bool IsAvailable { get; set; }

        [JsonIgnore]
        public virtual ICollection<AttachmentMessageRelation> LinkedMessages { get; set; }
    }

    public class AttachmentMessageRelation
    {
        public Ulid AttachmentId { get; set; }


        public Ulid MsgId { get; set; }

        [JsonIgnore]
        public HatoAttachment Attachment { get; set; }
        [JsonIgnore]
        public HatoMessage Message { get; set; }
    }

    public class HatoMessageAbbr
    {

        public Ulid MsgId { get; set; }

        public HatoChannel? _Channel { get; set; } = null;


        public Ulid ChannelId { get; set; }

        public DateTimeOffset Timestamp { get => ChannelId.Time; }

        public string SenderEmail { get; set; }
        public string? SenderNickname { get; set; }

        // public List<Address> Receivers { get; set; }

        public string? Title { get; set; }

    }

    public class HatoMessage : HatoMessageAbbr
    {

        public string? BodyPlain { get; set; }
        public string BodyHtml { get; set; }

        [JsonIgnore]
        public virtual ICollection<AttachmentMessageRelation> LinkedAttachments { get; set; }

        [NotMapped]
        public IList<HatoAttachment> Attachments { get; set; }

        [JsonIgnore]
        public NpgsqlTsVector? tsvector { get; set; }

        public List<string> Tags { get; set; }
    }

    public class RecentMessageViewItem
    {
        public Ulid ChannelId { get; set; }

        public string ChannelTitle { get; set; }

        public Ulid? MsgId { get; set; }

        public string? BodyPlain { get; set; }

        public string? Title { get; set; }

        public string? SenderEmail { get; set; }

        public string? SenderNickname { get; set; }
    }

    public class HatoChannel
    {

        public Ulid ChannelId { get; set; }

        public string ChannelUsername { get; set; }

        public string ChannelTitle { get; set; }

        public bool IsPublic { get; set; }

        [JsonIgnore]
        public virtual ICollection<ChannelUserRelation> _Users { get; set; }
        [JsonIgnore]
        public virtual ICollection<HatoMessage> _Messages { get; set; }

        // [JsonIgnore]
        // public virtual ICollection<ChannelRole> _Roles { get; set; }
        // [JsonIgnore]
        public virtual ICollection<InvitationLink> _InvitationLinks { get; set; }

        public UserPermission DefaultPermission { get; set; }
            = UserPermission.Receive;
    }

    public class ChannelUserRelation
    {
        public Ulid UserId { get; set; }
        public virtual User _User { get; set; }

        public Ulid ChannelId { get; set; }
        public virtual HatoChannel _Channel { get; set; }

        public bool ShouldReceiveMessage { get; set; }
        public bool IsCreator { get; set; }

        public UserPermission Permission { get; set; }
    }

    public class User
    {
        public Ulid UserId { get; set; }

        public string? Nickname { get; set; }

        public string Email { get; set; }

        public virtual ICollection<ChannelUserRelation> _Channels { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserLoginCode>? _LoginCodes { get; set; } = null;
    }

    public class UserLoginCode
    {
        public Ulid CodeId { get; set; }

        public string Code { get; set; }

        public DateTimeOffset Expires { get; set; }

        // =============


        /// <summary>
        /// How long is the code?
        /// </summary>
        public const int CodeLength = 6;
        public static readonly char[] usableLetters = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        public static readonly TimeSpan ExpirationTime = new TimeSpan(0, 15, 0);

        public static UserLoginCode Generate(DateTimeOffset timestamp)
        {
            var rng = new System.Random();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < CodeLength; i++)
            {
                sb.Append(usableLetters[rng.Next(usableLetters.Length)]);
            }
            return new UserLoginCode()
            {
                CodeId = Ulid.NewUlid(),
                Code = sb.ToString(),
                Expires = timestamp + ExpirationTime
            };
        }
    }

    public class InvitationLink
    {
        public Ulid ChannelId { get; set; }

        public string LinkId { get; set; }

        public UserPermission DefaultPermission { get; set; }
            = UserPermission.Receive;

        public DateTimeOffset Expires { get; set; }
            = DateTimeOffset.MaxValue;

        public const int LinkLength = 24;
        public static string GenerateLinkId(DateTimeOffset timestamp, Random rng)
        {
            const int longbytes = sizeof(long);
            Span<byte> linkBytes = stackalloc byte[LinkLength];
            {
                var timeSpan = linkBytes.Slice(0, longbytes);
                var milliseconds = timestamp.ToUnixTimeMilliseconds();
                BitConverter.TryWriteBytes(timeSpan, milliseconds);
                if (BitConverter.IsLittleEndian) timeSpan.Reverse();
            }
            {
                var randomSpan = linkBytes.Slice(longbytes);
                rng.NextBytes(randomSpan);
            }
            return Convert.ToBase64String(linkBytes).Replace('+', '-').Replace('/', '_');
        }
    }


    // Disable initialization warning because we don't need that for now

    public class EmailSystemContext : DbContext
    {
        // ========= Tables =========
        public DbSet<HatoMessage> Messages { get; set; }
        public DbSet<HatoChannel> Channels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChannelUserRelation> ChannelUserTable { get; set; }
        public DbSet<HatoAttachment> Attachments { get; set; }
        public DbSet<AttachmentMessageRelation> AttachmentRelations { get; set; }
        public DbSet<InvitationLink> InvitationLinks { get; set; }


        // ========= Views =========
        public DbSet<RecentMessageViewItem> RecentMessages { get; set; }

        public EmailSystemContext(DbContextOptions ctx) : base(ctx)
        {
        }

        public static ValueConverter<Ulid, Guid> UlidGuidConverter = new ValueConverter<Ulid, Guid>(
            ulid => ulid.ToGuid(),
            guid => new Ulid(guid)
        );

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureHatoMessage(modelBuilder);
            ConfigureUser(modelBuilder);
            ConfigureChannel(modelBuilder);
            ConfigureAttachment(modelBuilder);
            ConfigureChannelUserRelation(modelBuilder);
            ConfigureAttachmentMessageRelation(modelBuilder);
            ConfigureRecentMessageView(modelBuilder);
            ConfigureInvitationLinks(modelBuilder);
            modelBuilder.ToSnakeCase();
        }

        private void ConfigureRecentMessageView(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecentMessageViewItem>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<RecentMessageViewItem>().Property(x => x.MsgId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<RecentMessageViewItem>().HasNoKey().ToView("recent_messages");
            modelBuilder.Entity<RecentMessageViewItem>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<RecentMessageViewItem>().HasIndex(x => x.MsgId);
        }

        private void ConfigureInvitationLinks(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvitationLink>().HasKey(x => x.LinkId);
            modelBuilder.Entity<InvitationLink>().HasIndex(x => x.LinkId);
            modelBuilder.Entity<InvitationLink>().HasOne<HatoChannel>().WithMany(x => x._InvitationLinks).HasForeignKey(x => x.ChannelId);
            modelBuilder.Entity<InvitationLink>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<InvitationLink>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
        }

        private void ConfigureAttachmentMessageRelation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentMessageRelation>().HasKey(x => new { x.AttachmentId, x.MsgId });
            modelBuilder.Entity<AttachmentMessageRelation>().HasIndex(x => new { x.AttachmentId, x.MsgId });
            modelBuilder.Entity<AttachmentMessageRelation>().HasIndex(x => x.AttachmentId);
            modelBuilder.Entity<AttachmentMessageRelation>().HasIndex(x => x.MsgId);
            modelBuilder.Entity<AttachmentMessageRelation>().HasOne(x => x.Message).WithMany(u => u.LinkedAttachments).HasForeignKey(u => u.MsgId);
            modelBuilder.Entity<AttachmentMessageRelation>().HasOne(x => x.Attachment).WithMany(u => u.LinkedMessages).HasForeignKey(u => u.AttachmentId);
            modelBuilder.Entity<AttachmentMessageRelation>().Property(x => x.AttachmentId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<AttachmentMessageRelation>().Property(x => x.MsgId).HasConversion(UlidGuidConverter);
        }

        private void ConfigureChannelUserRelation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelUserRelation>().HasKey(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._User).WithMany(u => u._Channels).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._Channel).WithMany(u => u._Users).HasForeignKey(u => u.ChannelId);
            modelBuilder.Entity<ChannelUserRelation>().Property(x => x.UserId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<ChannelUserRelation>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
        }

        private void ConfigureAttachment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatoAttachment>().HasKey(x => x.AttachmentId);
            modelBuilder.Entity<HatoAttachment>().HasIndex(x => x.AttachmentId);
            modelBuilder.Entity<HatoAttachment>().Property(x => x.AttachmentId).HasConversion(UlidGuidConverter);
        }

        private void ConfigureChannel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatoChannel>().HasKey(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelId);
            modelBuilder.Entity<HatoChannel>().HasIndex(x => x.ChannelUsername);
            modelBuilder.Entity<HatoChannel>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(x => x.UserId);
            modelBuilder.Entity<User>().HasIndex(x => x.UserId);
            modelBuilder.Entity<User>().Property(x => x.UserId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<User>().HasAlternateKey(x => x.Email);
            modelBuilder.Entity<User>().HasIndex(x => x.Email);
            modelBuilder.Entity<User>().OwnsMany(x => x._LoginCodes, code =>
            {
                code.WithOwner().HasForeignKey("UserId");
                code.Property(x => x.CodeId).HasConversion(UlidGuidConverter);
                code.HasKey(x => x.CodeId);
                code.HasIndex(x => x.Code);
                code.HasIndex(x => x.Expires);
            });
        }

        private void ConfigureHatoMessage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatoMessage>().HasKey(x => x.MsgId);
            modelBuilder.Entity<HatoMessage>().Property(x => x.MsgId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<HatoMessage>().HasOne(x => x._Channel).WithMany(x => x._Messages).HasForeignKey(x => x.ChannelId);
            modelBuilder.Entity<HatoMessage>().Property(x => x.ChannelId).HasConversion(UlidGuidConverter);
            modelBuilder.Entity<HatoMessage>().HasIndex(x => x.MsgId);
            modelBuilder.Entity<HatoMessage>().HasIndex(x => x.tsvector).HasMethod("GIN");
        }
    }
#pragma warning restore CS8618
}
