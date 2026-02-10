using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Controllers
{
    public class OrderController() : Controller
    {
        [HttpPost]
        public IActionResult CreateOrder([FromBody] Test test)
        {
            return View();
        }        
    }
    public class  Test 
    {
       public int ScreeningId { get; set; }
       public List<int> PlaceIds { get; set; }
    }
}
    