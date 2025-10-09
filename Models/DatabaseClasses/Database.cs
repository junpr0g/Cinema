using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Cinema.Models.DatabaseClasses;

public partial class Database : DbContext
{
    public Database()
    {
    }

    public Database(DbContextOptions<Database> options)
        : base(options)
    {
    }
    public virtual DbSet<Genre> Genres { get; set; }
    public virtual DbSet<Localisation> Localisations { get; set; }
    public virtual DbSet<AgeLimit> AgeLimits { get; set; }
    public virtual DbSet<Movie> Movies { get; set; }
    public virtual DbSet<MovieScreening> MovieScreenings { get; set; }
    public virtual DbSet<Hall> Halls { get; set; }
    public virtual DbSet<Row> Rows { get; set; }
    public virtual DbSet<Place> Places { get; set; }
    public virtual DbSet<Viewer> Viewers { get; set; }
    public virtual DbSet<Ticket> Tickets { get; set; }
    public virtual DbSet<Cinema> Cinemas { get; set; }
    public virtual DbSet<SeatType> SeatTypes { get; set; }
    public virtual DbSet<Reserved> Reserveds { get; set; }
    public virtual DbSet<ContactInformation> ContactInformations { get; set; }
    public virtual DbSet<MovieRental> MovieRentals { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseLazyLoadingProxies().UseMySql("server=localhost;user=root;password=toor;database=cinema", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
