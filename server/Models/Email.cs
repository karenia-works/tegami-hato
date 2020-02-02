using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace Karenia.TegamiHato.Server.Models
{
    // Disable initialization warning because we don't need that for now
#pragma warning disable CS8618
    public struct Address
    {
        public string Email { get; set; }
        public string Nickname { get; set; }
    }

    public struct Attachment
    {
        public string Filename { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }

    public class HatoEmail
    {

        public Guid EmailId { get; set; }

        public string Channel { get; set; }

        public DateTime Timestamp { get; set; }

        public Address Sender { get; set; }

        public List<Address> Receivers { get; set; }

        public string Title { get; set; }

        public string BodyPlain { get; set; }

        public string? BodyHtml { get; set; }

        public List<Attachment> attachments { get; set; }
    }

    public class HatoChannel
    {
        public Guid ChannelId { get; set; }

        public string ChannelUsername { get; set; }

        public string Title { get; set; }

        public List<ChannelUserRelation> _Users { get; set; }
    }

    public class ChannelUserRelation
    {
        public Guid UserId { get; set; }
        public User _User { get; set; }

        public Guid ChannelId { get; set; }
        public HatoChannel _Channel { get; set; }

        public bool IsAdmin { get; set; }
    }

    public class User
    {
        public Guid UserId { get; set; }

        public List<ChannelUserRelation> _Channels { get; set; }
    }

    // Disable initialization warning because we don't need that for now

    public class EmailSystemContext : DbContext
    {
        public DbSet<HatoEmail> Emails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChannelUserRelation> ChannelUserTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HatoEmail>().HasKey(x => x.EmailId);
            modelBuilder.Entity<User>().HasKey(x => x.UserId);

            modelBuilder.Entity<ChannelUserRelation>().HasKey(x => new { x.UserId, x.ChannelId });
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._User).WithMany(u => u._Channels).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<ChannelUserRelation>().HasOne(x => x._Channel).WithMany(u => u._Users).HasForeignKey(u => u.ChannelId);
        }
    }
#pragma warning restore CS8618
}
