using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Controllers
{
    public class OrderController(Database db) : Controller
    {
        [HttpPost]
        public IActionResult CreateOrder()         
        {
            return View();
        }
    }
}
    