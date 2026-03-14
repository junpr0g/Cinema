using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
namespace Cinema.Controllers
{
    public class OrderController(Database db) : Controller
    {
        [HttpGet]
        [Authorize]
        public async IActionResult CreateOrder([FromQuery] int screeningId, [FromQuery] List<int> placeIds)
        {
            List<int> NewReservedIds = new List<int>();
            if (placeIds != null || placeIds.Any())
            {
                // Первым делом проверяем, что бы выбранные места были свободны.
                MovieScreening screening = db.MovieScreenings.FirstOrDefault(s => s.Id == screeningId);
                List<Reserved> AllScreeningPlaces = db.Reserveds.Where(r => r.MovieScreeningId == screeningId && r.IsUsed == true).ToList();
                bool isAnyPlaceReserved = false; // Если true, значит одно из мест уже занято, и нужно показать ошибку пользователю, что бы он выбрал другие места.

                foreach (int placeId in placeIds) 
                {
                    if (AllScreeningPlaces.Any(r => r.PlaceId == placeId))
                    {
                        isAnyPlaceReserved = true;
                        break;
                    }
                }
                
                    // Получаем пользователя из cookie.
                    Viewer user = db.Viewers.FirstOrDefault(v => v.Email == User.Identity.Name);
                // Оформляем заказ в базе, связывая его с пользователем, сеансом и местами.
                if (user != null && isAnyPlaceReserved == false) 
                {
                    foreach (int placeId in placeIds) 
                    {
                        Reserved reserved = new Reserved();
                        reserved.MovieScreeningId = screeningId;
                        reserved.IsUsed = true;
                        reserved.PlaceId = placeId;
                        reserved.ViewerId = user.Id;
                        reserved.CreatedAt = DateTime.Now;
                        reserved.ExpireAt = DateTime.Now.AddMinutes(15); // Время, в течение которого бронь будет действительна (например, 15 минут).
                        NewReservedIds.Add(db.Reserveds.Add(reserved).Context.Entry(reserved).Entity.Id);
                    }
                    await db.SaveChangesAsync(); 
                    // Генерируем билет с возможностью его скачать(код взять из другого проекта).

                }   

               




                return BadRequest(new { success = false, message = "Нет выбранных мест" });
            }

            // TODO: здесь создаём заказ в базе (используя screeningId и placeIds)
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
}
    