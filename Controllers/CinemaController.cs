using Cinema.Models;
using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Cinema.Controllers
{
    public class CinemaController : Controller
    {
        private readonly ILogger<CinemaController> _logger;
        private readonly Database _db;

        public CinemaController(ILogger<CinemaController> logger, Database db)
        {
            _logger = logger;
            _db = db;
        }
        public IActionResult Main()
        {
            List<MovieRental> MovieRentals = _db.MovieRentals.Where(t=>t.RentalEnd>DateTime.Now).ToList();
            return View(MovieRentals);
        }
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Details(int id)
        {
            Movie movie = _db.Movies.FirstOrDefault(t => t.Id == id);     
            List<MovieScreening> screening = _db.MovieScreenings.Where(t => t.MovieId == id).ToList();
            ViewBag.Screening = screening;
            return View(movie);
        }
        [HttpGet]
        public IActionResult PartialCheckScreeningData(int id) 
        {
            MovieScreening movieScreening = _db.MovieScreenings.Include(t => t.Hall).FirstOrDefault(t => t.Id == id);
            List<Reserved> reserveds = _db.Reserveds.Where(t => t.MovieScreeningId == id).ToList();
            ViewBag.Reserved = reserveds;
            return PartialView(movieScreening);
        }
    }
}
