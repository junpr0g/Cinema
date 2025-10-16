using Cinema.Models.DatabaseClasses;
using Cinema.Models.ValidationModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
