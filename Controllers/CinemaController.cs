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

        public CinemaController(ILogger<CinemaController> logger)
        {
            _logger = logger;
        }
        public IActionResult Main()
        {
            Database db = new Database();       
            List<MovieRental> MovieRentals = db.MovieRentals.Where(t=>t.RentalEnd>DateTime.Now).ToList();
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
            Database db = new Database();
            Movie movie = db.Movies.FirstOrDefault(t => t.Id == id);     
            List<MovieScreening> screening = db.MovieScreenings.Where(t => t.MovieId == id).ToList();
            ViewBag.Screening = screening;
            return View(movie);
        }
        [HttpGet]
        public IActionResult PartialCheckScreeningData(int id) 
        {
            Database db = new Database();
            MovieScreening movieScreening = db.MovieScreenings.FirstOrDefault(t => t.Id == id);
            return PartialView(movieScreening);          
        }
    }
}
