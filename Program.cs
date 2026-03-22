using Cinema.Models.DatabaseClasses;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using QuestPDF.Infrastructure;

namespace Cinema
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            QuestPDF.Settings.License = LicenseType.Community;
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Регистрация DbContext в DI
            builder.Services.AddDbContext<Database>();

            // Настройка аутентификации Google
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrWhiteSpace(Cinema.Services.TicketService.secretKey))
            {
                Cinema.Services.TicketService.secretKey = builder.Configuration["Ticket:Secretkey"];
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.CallbackPath = "/signin-google";
                    options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
                    options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
                    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
                    options.SaveTokens = true; // Это критически важно
                });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Cinema/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();    // аутентификация

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Cinema}/{action=Main}/{id?}");

            app.Run();
        }
    }
}
