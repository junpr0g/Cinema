using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Cinema.Controllers
{
    public class OrderController(Database db) : Controller
    {
        [HttpGet]
        [Authorize] 
        public IActionResult CreateOrder([FromBody] OrderCreator order)
        {
            if (order == null || order.PlaceIds == null || !order.PlaceIds.Any())
            {
                return BadRequest(new { success = false, message = "Нет выбранных мест" });
            }

            // TODO: здесь создаём заказ в базе (используя ScreeningId и PlaceIds)
            // Пока вернём заглушку
            var fakeOrderId = 1;

            return Json(new
            {
                success = true,
                orderId = fakeOrderId,
                redirectUrl = Url.Action("Main", "Cinema")
            });
        }
    }

    public class OrderCreator
    {
        public int ScreeningId { get; set; }
        public List<int> PlaceIds { get; set; }
    }
}
    