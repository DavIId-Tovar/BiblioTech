using System;
using System.Collections.Generic;
using BiblioTech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Data;

public partial class BibliotecaContext : DbContext
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Autor> Autores { get; set; }

    public virtual DbSet<Libro> Libros { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<Socio> Socios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwPrestamosVencido> VwPrestamosVencidos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autor>(entity =>
        {
            entity.Property(e => e.Nacionalidad).HasMaxLength(60);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasIndex(e => e.Titulo, "IX_Libros_Titulo");

            entity.HasIndex(e => e.Isbn, "UQ_Libros_ISBN").IsUnique();

            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasMany(d => d.Autores).WithMany(p => p.Libros)
                .UsingEntity<Dictionary<string, object>>(
                    "LibroAutor",
                    r => r.HasOne<Autor>().WithMany()
                        .HasForeignKey("AutorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_LibroAutor_Autores"),
                    l => l.HasOne<Libro>().WithMany()
                        .HasForeignKey("LibroId")
                        .HasConstraintName("FK_LibroAutor_Libros"),
                    j =>
                    {
                        j.HasKey("LibroId", "AutorId");
                        j.ToTable("LibroAutor");
                        j.HasIndex(new[] { "AutorId" }, "IX_LibroAutor_AutorId");
                    });
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasIndex(e => e.LibroId, "IX_Prestamos_LibroId");

            entity.HasIndex(e => new { e.SocioId, e.Estado }, "IX_Prestamos_SocioId_Estado");

            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValue("Activo", "DF_Prestamos_Estado");
            entity.Property(e => e.FechaPrestamo).HasDefaultValueSql("(sysdatetime())", "DF_Prestamos_FechaPrestamo");

            entity.HasOne(d => d.Libro).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.LibroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prestamos_Libros");

            entity.HasOne(d => d.Socio).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.SocioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prestamos_Socios");
        });

        modelBuilder.Entity<Socio>(entity =>
        {
            entity.HasIndex(e => e.Documento, "UQ_Socios_Documento").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_Socios_Email").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Socios_Activo");
            entity.Property(e => e.Documento).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())", "DF_Socios_FechaRegistro");
            entity.Property(e => e.Nombre).HasMaxLength(120);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Usuarios_Email").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValue("Bibliotecario", "DF_Usuarios_Rol");
        });

        modelBuilder.Entity<VwPrestamosVencido>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PrestamosVencidos");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Socio).HasMaxLength(120);
            entity.Property(e => e.Titulo).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
