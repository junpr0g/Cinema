using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Cinema.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace Cinema.Controllers
{
    public class OrderController(Database db) : Controller
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromQuery] int screeningId, [FromQuery] List<int> placeIds)
        {
            bool a = User.Identity.IsAuthenticated;
            // 1. Валидация входных данных
            if (placeIds == null || !placeIds.Any())
                return BadRequest(new { success = false, message = "Нет выбранных мест" });

            // 2. Загружаем сеанс вместе с данными фильма
            var screening = await db.MovieScreenings
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
                return BadRequest(new { success = false, message = "Сеанс не найден" });

            // 3. Проверяем занятые места
            var conflicted = await db.Reserveds
                .AnyAsync(r => r.MovieScreeningId == screeningId && r.IsUsed && placeIds.Contains(r.PlaceId));

            if (conflicted)
                return BadRequest(new { success = false, message = "Одно или несколько мест уже заняты" });

            // 4. Получаем пользователя
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var user = await db.Viewers.FirstOrDefaultAsync(v => v.Email == email);
            if (user == null) return Unauthorized();

            // 5. Создаём брони и билеты (связываем их сразу в памяти)
            var newTickets = new List<Ticket>();

            // Заранее подгружаем информацию о местах, чтобы не делать запросы в цикле
            var places = await db.Places
                .Include(p => p.Row)
                .Where(p => placeIds.Contains(p.Id))
                .ToListAsync();

            foreach (var placeId in placeIds)
            {
                var place = places.FirstOrDefault(p => p.Id == placeId);

                var reserved = new Reserved
                {
                    MovieScreeningId = screeningId,
                    IsUsed = true,
                    PlaceId = placeId,
                    ViewerId = user.Id,
                    CreatedAt = DateTime.Now,
                    ExpireAt = DateTime.Now.AddMinutes(15),
                    Place = place // Привязываем объект для дальнейшей генерации PDF
                };

                var ticket = new Ticket
                {
                    ViewerId = user.Id,
                    MovieScreeningId = screeningId,
                    Reserved = reserved,
                    SeatTypeId = place?.SeatTypeId ?? 0
                };

                db.Reserveds.Add(reserved);
                db.Tickets.Add(ticket);
                newTickets.Add(ticket); // Отслеживаем только новые билеты
            }

            // 6. ПЕРВОЕ СОХРАНЕНИЕ: После этого у всех ticket появятся Id
            await db.SaveChangesAsync();

            // 7. Настройка сервиса билетов
            var ticketService = new Cinema.Services.TicketService();
            if (string.IsNullOrWhiteSpace(Cinema.Services.TicketService.secretKey))
            {
                Cinema.Services.TicketService.secretKey = Environment.GetEnvironmentVariable("TICKET_SECRET") ?? "0123456789ABCDEF0123456789ABCDEF";
            }

            // 8. Генерация PDF для новых билетов
            var ticketsForView = new List<TicketDto>();
            foreach (var ticket in newTickets)
            {
                byte[] pdfBytes = null;
                var currentPlace = ticket.Reserved.Place;

                try
                {
                    // Теперь ticket.Id гарантированно получен из БД
                    pdfBytes = ticketService.Generate(
                        ticket.Id,
                        screening.Movie.Name,
                        currentPlace.PlaceNumber,
                        currentPlace.Row.RowNumber,
                        screening.BeginningTime,
                        currentPlace.VIP,
                        user.Surname +" "+user.Name
                    );
                }
                catch (Exception ex)
                {
                    // Здесь можно добавить логгер
                }

                var pdfBase64 = pdfBytes != null ? Convert.ToBase64String(pdfBytes) : null;

                ticketsForView.Add(new TicketDto(
                    pdfBase64,
                    ticket,
                    currentPlace
                ));
            }

            // Возвращаем View только с текущим заказом
            return View("CreateOrder", ticketsForView);
        }
    }
}