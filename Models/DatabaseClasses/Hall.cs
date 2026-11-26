namespace Cinema.Models.DatabaseClasses
{
    public class Hall
    {   
        public int Id { get; set; }
        public int Number { get; set; }
        public virtual List<Row> Rows { get; set; }
    }
}
