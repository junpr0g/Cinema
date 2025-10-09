namespace Cinema.Models.DatabaseClasses
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan Timing { get; set; }
        public string TrailerPath { get; set; }
        public string PosterPath { get; set; }
        public int AgeLimitId { get; set; }
        public virtual AgeLimit AgeLimit { get; set; }
        //public int LocalisationId { get; set; }
        public virtual List<Localisation> Localisations { get; set; }
        //public int GenreId { get; set; }
        public virtual List<Genre> Genres { get; set; }
    }
}
