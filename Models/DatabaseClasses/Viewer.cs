using Microsoft.AspNetCore.Authentication;

namespace Cinema.Models.DatabaseClasses
{
    public class Viewer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }   
        public DateTime Birthday { get; set; }
        public string Email { get; set; } 
        public string Password { get; set; }
    }
}
