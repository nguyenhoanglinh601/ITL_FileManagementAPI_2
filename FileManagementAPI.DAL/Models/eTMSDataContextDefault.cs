using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileManagementAPI.Service.Models
{
    public partial class eTMSDataContextDefault : DbContext
    {
        public eTMSDataContextDefault()
        {
        }

        public eTMSDataContextDefault(DbContextOptions<eTMSDataContextDefault> options)
            : base(options)
        {
        }

        public virtual DbSet<CsDocument> CsDocument { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=S3Test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<CsDocument>(entity =>
            {
                entity.ToTable("csDocument");

                entity.HasIndex(e => new { e.BranchId, e.ReferenceObject, e.DocType, e.FileName, e.StorageFileName })
                    .HasName("U_Document")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.DocType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FileCheckSum).HasMaxLength(100);

                entity.Property(e => e.FileDescription).HasMaxLength(500);

                entity.Property(e => e.FileExtension).HasMaxLength(10);

                entity.Property(e => e.FileName).HasMaxLength(150);

                entity.Property(e => e.Icon).HasColumnType("image");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.ReferenceObject).HasMaxLength(100);

                entity.Property(e => e.StorageFileName).HasMaxLength(150);

                entity.Property(e => e.StorageOriginVersionId)
                    .HasColumnName("StorageOriginVersionID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StorageVersionId)
                    .HasColumnName("StorageVersionID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
        }
    }
}
