using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using ABC_Retail_Part1.Helpers;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ABC_Retail_Part1.Services;
using ABC_Retail_Part1.Models;

namespace ABC_Retail_Part1.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _conn;
        private readonly TableService _tableService;

        // Hardcoded admin credentials
        private const string AdminEmail = "Admin@gmail.com";
        private const string AdminPassword = "Admin123";

        public AccountController(IConfiguration config, TableService tableService)
        {
            _conn = config.GetConnectionString("DefaultConnection");
            _tableService = tableService;
        }

        private void SignInSession(int customerId, string name, string email, string role)
        {
            var userObj = new { CustomerId = customerId, Name = name, Email = email, Role = role };
            HttpContext.Session.SetString("CurrentUser", JsonSerializer.Serialize(userObj));
        }

        private void SignOutSession()
        {
            HttpContext.Session.Remove("CurrentUser");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string returnUrl = null)
        {
            // Check hardcoded admin first
            if (email == AdminEmail && password == AdminPassword)
            {
                SignInSession(0, "Admin", AdminEmail, "Admin");
                return RedirectToAction("Index", "Home");
            }

            // SQL login for normal users
            using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT CustomerId, Name, Email, PasswordHash, Role FROM dbo.Customers WHERE Email = @email", con);
            cmd.Parameters.AddWithValue("@email", email);

            using var rdr = await cmd.ExecuteReaderAsync();
            if (!rdr.HasRows)
            {
                ModelState.AddModelError("", "Invalid login");
                return View();
            }

            await rdr.ReadAsync();
            var id = Convert.ToInt32(rdr["CustomerId"]);
            var name = rdr["Name"]?.ToString();
            var storedHash = rdr["PasswordHash"]?.ToString();
            var role = rdr["Role"]?.ToString() ?? "Customer";

            if (storedHash == null || !PasswordHelper.VerifyPassword(password, storedHash))
            {
                ModelState.AddModelError("", "Invalid login");
                return View();
            }

            // Sign in to session
            SignInSession(id, name, email, role);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password != confirmPassword)
            {
                ModelState.AddModelError("", "Invalid input");
                return View();
            }

            // Prevent registering as admin
            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Cannot register as admin");
                return View();
            }

            var hash = PasswordHelper.HashPassword(password);

            using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            // Check if email already exists in SQL
            var checkCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Customers WHERE Email = @Email", con);
            checkCmd.Parameters.AddWithValue("@Email", email);
            var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;
            if (exists)
            {
                ModelState.AddModelError("", "Email already registered");
                return View();
            }

            // Insert into SQL Customers table
            var sqlCmd = new SqlCommand(
                "INSERT INTO dbo.Customers (Name, Email, PasswordHash, Role) OUTPUT INSERTED.CustomerId " +
                "VALUES (@Name, @Email, @PasswordHash, @Role)", con);
            sqlCmd.Parameters.AddWithValue("@Name", name);
            sqlCmd.Parameters.AddWithValue("@Email", email);
            sqlCmd.Parameters.AddWithValue("@PasswordHash", hash);
            sqlCmd.Parameters.AddWithValue("@Role", "Customer");
            await sqlCmd.ExecuteScalarAsync();

            // Insert into Azure Table so admin can see
            var tableCustomer = new Customer
            {
                PartitionKey = "Customer",
                RowKey = Guid.NewGuid().ToString(),
                Name = name,
                Email = email
            };
            await _tableService.InsertOrUpdateAsync("Customer", tableCustomer);

            TempData["Message"] = "Registered successfully!";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            SignOutSession();
            return RedirectToAction("Index", "Home");
        }
    }
}
