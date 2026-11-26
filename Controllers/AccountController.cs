using Cinema.Models.DatabaseClasses;
using Cinema.Models.ValidationModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Cinema.Controllers
{
    public class AccountController(Database db) : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction("Main", "Cinema");
        }

        [HttpPost]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            // Запрашиваем offline доступ, чтобы получить access_token для People API
            properties.Items.Add("prompt", "consent");
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            // 1. Получаем результат аутентификации от внешней службы (Google)
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                        ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            // --- БЛОК ПОЛУЧЕНИЯ ДОП. ДАННЫХ ЧЕРЕЗ PEOPLE API ---
            string phoneNumber = "";
            DateTime birthday = DateTime.Now.AddYears(-18);

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=phoneNumbers,birthdays");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("phoneNumbers", out var phones) && phones.GetArrayLength() > 0)
                        {
                            phoneNumber = phones[0].GetProperty("value").GetString() ?? "";
                        }

                        if (doc.RootElement.TryGetProperty("birthdays", out var birthdays) && birthdays.GetArrayLength() > 0)
                        {
                            var datePart = birthdays[0].GetProperty("date");
                            int year = datePart.TryGetProperty("year", out var y) ? y.GetInt32() : DateTime.Now.Year - 18;
                            int month = datePart.GetProperty("month").GetInt32();
                            int day = datePart.GetProperty("day").GetInt32();
                            birthday = new DateTime(year, month, day);
                        }
                    }
                }
            }

            // 2. Работа с БД
            Viewer viewer = await db.Viewers.FirstOrDefaultAsync(v => v.Email == email);

            if (viewer == null)
            {
                viewer = new Viewer
                {
                    Email = email,
                    Name = givenName ?? name ?? "Пользователь",
                    Surname = surname ?? "",
                    Password = "", // Для Google не нужен
                    Birthday = birthday,
                    PhoneNumber = phoneNumber
                };
                db.Viewers.Add(viewer);
            }
            else
            {
                // Обновляем данные только если они пустые
                if (string.IsNullOrEmpty(viewer.PhoneNumber)) viewer.PhoneNumber = phoneNumber;
                if (viewer.Birthday == DateTime.MinValue || viewer.Birthday.Year < 1920) viewer.Birthday = birthday;
            }

            await db.SaveChangesAsync();

            // 3. Авторизуем в системе с актуальными именами
            await Authenticate(viewer.Email, viewer.Name, viewer.Surname);

            return RedirectToAction("Main", "Cinema");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var viewer = await db.Viewers.FirstOrDefaultAsync(v => v.Email == model.Email && v.Password == model.Password);
                if (viewer != null)
                {
                    await Authenticate(viewer.Email, viewer.Name, viewer.Surname);
                    return RedirectToAction("Main", "Cinema");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                bool exists = await db.Viewers.AnyAsync(u => u.Email == model.Email);
                if (!exists)
                {
                    var viewer = new Viewer
                    {
                        Email = model.Email,
                        Password = model.Password,
                        Birthday = model.Birthday,
                        Name = model.Name,
                        Surname = model.Surname,
                        PhoneNumber = model.PhoneNumber
                    };
                    db.Viewers.Add(viewer);
                    await db.SaveChangesAsync();

                    await Authenticate(viewer.Email, viewer.Name, viewer.Surname);
                    return RedirectToAction("Main", "Cinema");
                }
                ModelState.AddModelError("", "Пользователь с таким Email уже существует.");
            }
            return View(model);
        }

        private async Task Authenticate(string userEmail, string firstName = "", string lastName = "")
        {
            var claims = new List<Claim>
            {
                // Имя пользователя (логин)
                new Claim(ClaimsIdentity.DefaultNameClaimType, userEmail),
                new Claim(ClaimTypes.Email, userEmail),
                // Добавляем имя и фамилию в клаймы, чтобы Layout мог их отобразить
                new Claim(ClaimTypes.GivenName, firstName ?? ""),
                new Claim(ClaimTypes.Surname, lastName ?? "")
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}