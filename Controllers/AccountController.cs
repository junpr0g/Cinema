using Cinema.Models.DatabaseClasses;
using Cinema.Models.ValidationModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        public async Task<IActionResult> GoogleResponse()
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
