namespace Cinema.Models.DatabaseClasses
{
    public class MovieRental
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public DateTime RentalStart { get; set; }
        public DateTime RentalEnd { get; set; }
    }
}
