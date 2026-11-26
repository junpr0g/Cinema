using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связи многие-ко-многим между Movie и Genre
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity<Dictionary<string, object>>(
                "MovieGenre", // Имя промежуточной таблицы
                j => j.HasOne<Genre>().WithMany().HasForeignKey("GenreId"),
                j => j.HasOne<Movie>().WithMany().HasForeignKey("MovieId"),
                j =>
                {
                    j.HasKey("MovieId", "GenreId");
                    j.ToTable("MovieGenres");
                });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
