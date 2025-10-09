namespace Cinema.Models.DatabaseClasses
{
    public class Cinema
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Adress { get; set; }
        public string Description { get; set; }
        public virtual List<ContactInformation> ContactInformations { get; set; }
        
    }
}
