using ArchiveDomain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ArchiveInfrastructure;

public partial class DbarchiveContext : IdentityDbContext<User>
{
    public DbarchiveContext(DbContextOptions<DbarchiveContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }
    public virtual DbSet<Form> Forms { get; set; }
    public virtual DbSet<Language> Languages { get; set; }
    public virtual DbSet<Poetry> Poetries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Важливо для Identity

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.Property(e => e.FormName).HasMaxLength(50);
            entity.HasMany(d => d.Poetries)
                  .WithMany(p => p.Forms)
                  .UsingEntity<Dictionary<string, object>>(
                      "FormsPoetry",
                      r => r.HasOne<Poetry>().WithMany()
                            .HasForeignKey("PoetryId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .HasConstraintName("FK_FormsPoetry_Poetry"),
                      l => l.HasOne<Form>().WithMany()
                            .HasForeignKey("FormId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .HasConstraintName("FK_FormsPoetry_Forms"),
                      j =>
                      {
                          j.HasKey("FormId", "PoetryId");
                          j.ToTable("FormsPoetry");
                          j.IndexerProperty<int>("FormId").HasColumnName("FormID");
                          j.IndexerProperty<int>("PoetryId").HasColumnName("PoetryID");
                      });
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.Property(e => e.Language1).HasMaxLength(50).HasColumnName("Language");
        });

        modelBuilder.Entity<Poetry>(entity =>
        {
            entity.ToTable("Poetry");
            entity.Property(e => e.AddedByUserId).HasColumnName("AddedByUserId");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.LanguageId).HasColumnName("LanguageID");
            entity.Property(e => e.PublicationDate).HasColumnType("datetime");
            entity.Property(e => e.Text).HasMaxLength(5000); // Збільшено ліміт для тексту
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.AddedByUser)
                  .WithMany(p => p.AddedPoems)
                  .HasForeignKey(d => d.AddedByUserId)
                  .OnDelete(DeleteBehavior.NoAction)
                  .HasConstraintName("FK_Poetry_Users");

            entity.HasOne(d => d.Author)
                  .WithMany(p => p.Poetries)
                  .HasForeignKey(d => d.AuthorId)
                  .OnDelete(DeleteBehavior.NoAction)
                  .HasConstraintName("FK_Poetry_Authors");

            entity.HasOne(d => d.Language)
                  .WithMany(p => p.Poetries)
                  .HasForeignKey(d => d.LanguageId)
                  .OnDelete(DeleteBehavior.NoAction)
                  .HasConstraintName("FK_Poetry_Languages");

            entity.HasMany(p => p.LikedByUsers)
                  .WithMany(u => u.LikedPoems)
                  .UsingEntity<Dictionary<string, object>>(
                      "PoetryLikes",
                      r => r.HasOne<User>().WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .HasConstraintName("FK_PoetryLikes_Users"),
                      l => l.HasOne<Poetry>().WithMany()
                            .HasForeignKey("PoetryId")
                            .OnDelete(DeleteBehavior.Cascade)
                            .HasConstraintName("FK_PoetryLikes_Poetry"),
                      j =>
                      {
                          j.HasKey("UserId", "PoetryId");
                          j.ToTable("PoetryLikes");
                          j.IndexerProperty<string>("UserId").HasColumnName("UserId");
                          j.IndexerProperty<int>("PoetryId").HasColumnName("PoetryId");
                      });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}