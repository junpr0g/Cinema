using Cinema.Models.DatabaseClasses;
using Cinema.Models.ValidationModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Cinema.Controllers
{
    public class AccountController : Controller
    {        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OldGoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            var givenName = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;
            var surname = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Surname)?.Value;
            
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            Database db = new Database();
            Viewer viewer = db.Viewers.FirstOrDefault(v => v.Email == email);
            
            if (viewer == null)
            {
                // Создаем нового пользователя из Google аккаунта
                viewer = new Viewer
                {
                    Email = email,
                    Name = givenName ?? name ?? "",
                    Surname = surname ?? "",
                    Password = "", // Пароль не нужен для Google авторизации
                    Birthday = DateTime.Now.AddYears(-18), // Значение по умолчанию
                    PhoneNumber = ""
                };
                db.Viewers.Add(viewer);
                await db.SaveChangesAsync();
            }

            await Authenticate(email);

            return RedirectToAction("Main", "Cinema");
        }
        public async Task<IActionResult> GoogleResponse()
        {
            // 1. Получаем результат аутентификации
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            var givenName = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;
            var surname = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            // --- БЛОК ПОЛУЧЕНИЯ ДОП. ДАННЫХ ЧЕРЕЗ PEOPLE API ---
            string phoneNumber = "";
            DateTime birthday = DateTime.Now.AddYears(-18); // Значение по умолчанию

            var accessToken = result.Properties.GetTokenValue("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=phoneNumbers,birthdays,profile");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        
                        
                        using var doc = JsonDocument.Parse(json);

                        // Достаем телефон (первый из списка)
                        if (doc.RootElement.TryGetProperty("phoneNumbers", out var phones))
                        {
                            phoneNumber = phones[0].GetProperty("value").GetString() ?? "";
                        }

                        // Достаем дату рождения
                        if (doc.RootElement.TryGetProperty("birthdays", out var birthdays))
                        {
                            var datePart = birthdays[0].GetProperty("date");
                            int year = datePart.TryGetProperty("year", out var y) ? y.GetInt32() : 2000;
                            int month = datePart.GetProperty("month").GetInt32();
                            int day = datePart.GetProperty("day").GetInt32();
                            birthday = new DateTime(year, month, day);
                        }
                    }
                }
            }
            // ---------------------------------------------------

            Database db = new Database(); // В идеале использовать DI (Dependency Injection)
            Viewer viewer = db.Viewers.FirstOrDefault(v => v.Email == email);

            if (viewer == null)
            {
                viewer = new Viewer
                {
                    Email = email,
                    Name = givenName ?? name ?? "Unknown",
                    Surname = surname ?? "",
                    Password = "",
                    Birthday = birthday, // Теперь тут реальная дата или дефолт
                    PhoneNumber = phoneNumber // И реальный телефон
                };
                db.Viewers.Add(viewer);
            }
            else
            {
                // По желанию: обновляем данные, если они пришли от Google и в базе были пустыми
                if (string.IsNullOrEmpty(viewer.PhoneNumber)) viewer.PhoneNumber = phoneNumber;
                if (viewer.Birthday == DateTime.MinValue) viewer.Birthday = birthday;
            }

            await db.SaveChangesAsync();
            await Authenticate(email);

            return RedirectToAction("Main", "Cinema");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Database db = new Database();
                Viewer viewer = db.Viewers.FirstOrDefault(v => v.Email == model.Email && v.Password == model.Password);
                if (viewer != null)
                {
                    await Authenticate(model.Email); // аутентификация

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
                Database db = new Database();
                Viewer viewer = db.Viewers.FirstOrDefault(u => u.Email == model.Email);
                if (viewer == null)
                {
                    // добавляем пользователя в бд
                    db.Viewers.Add(new Viewer { Email = model.Email, Password = model.Password, Birthday = model.Birthday, Name = model.Name, Surname = model.Surname, PhoneNumber = model.PhoneNumber });
                    await db.SaveChangesAsync();

                    await Authenticate(model.Email); // аутентификация

                    return RedirectToAction("Main", "Cinema");
                }
                else
                    ModelState.AddModelError("", "Пользователь с таким Email уже существует.");
            }
            return View(model);
        }

        private async Task Authenticate(string UserEmail)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, UserEmail)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
