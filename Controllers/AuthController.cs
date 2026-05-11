using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Water_Filtration.Models.data;

namespace Water_Filtration.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbPlcOnlineContext _context;
        public AuthController(DbPlcOnlineContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

       // Post: /Account/Login
       [HttpPost]
       
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Password == password);

            if (dbUser != null)
            {
                HttpContext.Session.SetInt32("UserId", dbUser.Id);   // ✅ store Id

                HttpContext.Session.SetString("Username", dbUser.UserName);
                return RedirectToAction("Dashboard", "Master");
            }
            else
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

           
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Remove all session data

            return RedirectToAction("Login", "Auth");
        }
    }
}
