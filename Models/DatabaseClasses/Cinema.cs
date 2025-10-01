namespace Cinema.Models.DatabaseClasses
{
    public class Cinema
    {
        public int Id { get; set; }
        public string Adress { get; set; }
        public int MoovieScreeningId { get; set; }
        public MovieScreening MovieScreening { get; set; }
        public int HallId { get; set; }
        public Hall Hall { get; set; }        
    }
}
