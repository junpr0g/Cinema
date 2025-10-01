namespace Cinema.Models.DatabaseClasses
{
    public class MovieScreening
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public DateTime BeginningTime {  get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int HallId { get; set; }
        public Hall Hall { get; set; }
    }
}
