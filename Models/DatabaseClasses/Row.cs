namespace Cinema.Models.DatabaseClasses
{
    public class Row
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public virtual List<Place> Places { get; set; }
    }
}
