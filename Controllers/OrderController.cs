using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Cinema.Models.DTOs;

namespace Cinema.Controllers

{
    public class OrderController(Database db) : Controller
    {
        [HttpGet]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> CreateOrder([FromQuery] int screeningId, [FromQuery] List<int> placeIds)
        {
            // Validate input
            if (placeIds == null || !placeIds.Any())
                return BadRequest(new { success = false, message = "Нет выбранных мест" });

            // Проверяем существование сеанса
            MovieScreening screening = db.MovieScreenings.FirstOrDefault(s => s.Id == screeningId);
            if (screening == null)
                return BadRequest(new { success = false, message = "Сеанс не найден" });

            // Проверяем занятые места на сеансе
            var allReserved = db.Reserveds.Where(r => r.MovieScreeningId == screeningId && r.IsUsed == true).ToList();
            var conflicted = placeIds.Intersect(allReserved.Select(r => r.PlaceId)).Any();
            if (conflicted)
                return BadRequest(new { success = false, message = "Одно или несколько мест уже заняты" });

            // Получаем пользователя
            var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value;

            Viewer user = db.Viewers.FirstOrDefault(v => v.Email == email);
            if (user == null)
                return Unauthorized();

            var createdReserved = new List<Reserved>();

            // Создаём брони
            foreach (int placeId in placeIds)
            {
                var reserved = new Reserved
                {
                    MovieScreeningId = screeningId,
                    IsUsed = true,
                    PlaceId = placeId,
                    ViewerId = user.Id,
                    CreatedAt = DateTime.Now,
                    ExpireAt = DateTime.Now.AddMinutes(15)
                };
                db.Reserveds.Add(reserved);
                createdReserved.Add(reserved);
            }

            await db.SaveChangesAsync();

            // Создаём сущности билетов и генерируем PDF'ы
            var ticketService = new Cinema.Services.TicketService();
            // Ensure secret key exists (fallback to a generated key if not set)
            if (string.IsNullOrWhiteSpace(Cinema.Services.TicketService.secretKey))
            {
                Cinema.Services.TicketService.secretKey = System.Environment.GetEnvironmentVariable("TICKET_SECRET") ?? "0123456789ABCDEF0123456789ABCDEF";
            }

            var ticketsForView = new List<TicketDto>();
            // Получить все ряды для текущего зала текущего сеанса            
            foreach (var reserved in createdReserved)
            {
                // Create ticket record
                var place = db.Places.FirstOrDefault(p => p.Id == reserved.PlaceId);
                var ticket = new Ticket
                {
                    ViewerId = user.Id,
                    MovieScreeningId = screeningId,
                    Reserved = reserved,   // ← EF сам заполнит ReservedId
                    SeatTypeId = place?.SeatTypeId ?? 0
                };
                db.Tickets.Add(ticket);
                // Prepare values for PDF
                var movieTitle = screening.Movie?.Name ?? "---";
                var placeNumber = place?.PlaceNumber ?? reserved.PlaceId;
                var isVip = place?.VIP ?? false;

                // Generate PDF bytes
                byte[] pdfBytes = null;
                try
                {
                    pdfBytes = ticketService.Generate(ticket.Id, movieTitle, placeNumber, 0, screening.BeginningTime, isVip);
                }
                catch
                {
                    // Fallback: attempt Generate with ticket numeric id = reserved.Id
                    pdfBytes = ticketService.Generate(reserved.Id, movieTitle, placeNumber, 0, screening.BeginningTime, isVip);
                }

                var pdfBase64 = pdfBytes != null ? Convert.ToBase64String(pdfBytes) : null;
                ticketsForView.Add(new TicketDto(
                    pdfBase64,
                    ticket,
                    reserved.Place.Row.RowNumber,
                    reserved.Place.PlaceNumber                   
                ));               
            }

            await db.SaveChangesAsync();

            // Возвращаем страницу с билетами (view уже создан пользователем)
            return View("CreateOrder", ticketsForView);
        }
    }
}
    