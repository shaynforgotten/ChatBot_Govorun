using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NP_Sockets_UPDProtocol
{
    public class ChatBotDbContext : DbContext
    {
        public DbSet<ResponseRecord> Responses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ChatBotResponses;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResponseRecord>()
                .ToTable("Responses")
                .HasKey(r => r.ID);

            modelBuilder.Entity<ResponseRecord>()
                .Property(r => r.Question)
                .IsRequired();

            modelBuilder.Entity<ResponseRecord>()
                .Property(r => r.Response)
                .IsRequired();

            modelBuilder.Entity<ResponseRecord>()
                .Property(r => r.Timestamp)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }

    [Table("Responses")]
    public class ResponseRecord
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string Response { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
