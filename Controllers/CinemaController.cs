using Cinema.Models;
using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Cinema.Controllers
{
    public class CinemaController : Controller
    {
        private readonly ILogger<CinemaController> _logger;

        public CinemaController(ILogger<CinemaController> logger)
        {
            _logger = logger;
        }

        public IActionResult Main()
        {
            Database db = new Database();
           

            List<MovieRental> MovieRentals = db.MovieRentals.Where(t=>t.RentalEnd>DateTime.Now).Include(t => t.Movie).ToList();

            return View(MovieRentals);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
