namespace Cinema.Models.DatabaseClasses
{
    public class MovieScreening
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public DateTime BeginningTime {  get; set; }
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public int HallId { get; set; }
        public virtual Hall Hall { get; set; }
    }
}
