namespace Cinema.Models.DatabaseClasses
{
    public class Place
    {
        public int Id { get; set; }
        public int PlaceNumber { get; set; }
        public bool VIP { get; set; }
        public int SeatTypeId { get; set; }
        public virtual SeatType SeatType { get; set; }
        public int RowId { get; set; }
        public virtual Row Row { get; set; }
    }
}
