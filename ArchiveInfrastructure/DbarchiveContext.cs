using System;
using System.Collections.Generic;
using ArchiveDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace ArchiveInfrastructure;

public partial class DbarchiveContext : DbContext
{
    public DbarchiveContext()
    {
    }

    public DbarchiveContext(DbContextOptions<DbarchiveContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Poetry> Poetries { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=HOME-PC\\SQLEXPRESS; Database=DBArchive; Trusted_Connection=True; TrustServerCertificate=True; ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.Property(e => e.FormName).HasMaxLength(50);

            entity.HasMany(d => d.Poetries).WithMany(p => p.Forms)
                .UsingEntity<Dictionary<string, object>>(
                    "FormsPoetry",
                    r => r.HasOne<Poetry>().WithMany()
                        .HasForeignKey("PoetryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FormsPoetry_Poetry"),
                    l => l.HasOne<Form>().WithMany()
                        .HasForeignKey("FormId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FormsPoetry_Forms"),
                    j =>
                    {
                        j.HasKey("FormId", "PoetryId");
                        j.ToTable("FormsPoetry");
                        j.IndexerProperty<int>("FormId").HasColumnName("FormID");
                        j.IndexerProperty<int>("PoetryId")
                            .ValueGeneratedOnAdd()
                            .HasColumnName("PoetryID");
                    });
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.Property(e => e.Language1)
                .HasMaxLength(50)
                .HasColumnName("Language");
        });

        modelBuilder.Entity<Poetry>(entity =>
        {
            entity.ToTable("Poetry");

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.LanguageId).HasColumnName("LanguageID");
            entity.Property(e => e.PublicationDate).HasColumnType("datetime");
            entity.Property(e => e.Text).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.Admin).WithMany(p => p.Poetries)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Poetry_Admins");

            entity.HasOne(d => d.Author).WithMany(p => p.Poetries)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Poetry_Authors");

            entity.HasOne(d => d.Language).WithMany(p => p.Poetries)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Poetry_Languages");
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);

            entity.HasMany(d => d.Poetries).WithMany(p => p.Readers)
                .UsingEntity<Dictionary<string, object>>(
                    "PoetryLike",
                    r => r.HasOne<Poetry>().WithMany()
                        .HasForeignKey("PoetryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PoetryLikes_Poetry"),
                    l => l.HasOne<Reader>().WithMany()
                        .HasForeignKey("ReaderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PoetryLikes_Readers"),
                    j =>
                    {
                        j.HasKey("ReaderId", "PoetryId");
                        j.ToTable("PoetryLikes");
                        j.IndexerProperty<int>("ReaderId")
                            .ValueGeneratedOnAdd()
                            .HasColumnName("ReaderID");
                        j.IndexerProperty<int>("PoetryId").HasColumnName("PoetryID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
