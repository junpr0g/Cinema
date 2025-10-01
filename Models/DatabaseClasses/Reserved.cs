namespace Cinema.Models.DatabaseClasses
{
    public class Reserved
    {
        public int Id { get; set; }
        // связь с сеансом
        public int MovieScreeningId { get; set; }
        public MovieScreening MovieScreening { get; set; }
        // кто забронировал
        public int ViewerId { get; set; } 
        // если система с авторизацией
        public Viewer Viewer { get; set; }

        // время действия брони
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; } 
        // например 10 мин на оплату                                              
        // связь с местом
        public int PlaceId { get; set; }
        public Place Place { get; set; } 
    }
}
