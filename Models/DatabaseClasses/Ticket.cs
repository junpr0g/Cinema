namespace Cinema.Models.DatabaseClasses
{
    public class Ticket
    {
        public string Id { get; set; }
        public int ViewerId { get; set; }
        public Viewer Viewer { get; set; }
        public int MovieScreeningId {  get; set; }
        public MovieScreening MovieScreening { get; set; }
        public int ReserverdId { get; set; }
        public Reserved Reserved { get; set; }
        public int SeatTypeId { get; set; }
        public SeatType SeatType { get; set; }
    }
}
