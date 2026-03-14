namespace Cinema.Models.DatabaseClasses
{
    public class Reserved
    {
        public int Id { get; set; }
        // связь с сеансом
        public int MovieScreeningId { get; set; }
        public virtual MovieScreening MovieScreening { get; set; }
        // кто забронировал
        public int ViewerId { get; set; } 
        // если система с авторизацией
        public virtual Viewer Viewer { get; set; }

        // время действия брони
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; } 
        // например 10 мин на оплату                                              
        // связь с местом
        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }
        public bool IsUsed { get; set; }

        //public Reserved( int movieScreeningId, int viewerId, DateTime createdAt, DateTime expireAt, int placeId, bool isUsed)
        //{
        //    MovieScreeningId = movieScreeningId;
        //    ViewerId = viewerId;
        //    CreatedAt = createdAt;
        //    ExpireAt = expireAt;
        //    PlaceId = placeId;
        //    IsUsed = isUsed;
        //}
    }

}
