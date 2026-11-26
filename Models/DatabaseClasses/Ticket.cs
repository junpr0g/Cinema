namespace Cinema.Models.DatabaseClasses
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ViewerId { get; set; }
        public virtual Viewer Viewer { get; set; }
        public int MovieScreeningId {  get; set; }
        public virtual MovieScreening MovieScreening { get; set; }
        public int ReserverdId { get; set; }
        public virtual Reserved Reserved { get; set; }
        public int SeatTypeId { get; set; }
        public virtual SeatType SeatType { get; set; }
    }
}
