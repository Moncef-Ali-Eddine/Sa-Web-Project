using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using test.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace projet_sa_web.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Login GET - Render login form
        public IActionResult Login()
        {
            return View();
        }

        // Login POST - Handle login form submission
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username or password is required.");
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // For this example, assume password is stored in plaintext (not recommended for production)
            if (user.Password_hash != password)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // Store username in session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect based on role
            if (user.Role == "Admin")
            {
                return RedirectToAction("AdminView", "User");
            }
            else if (user.Role == "Employee")
            {
                return RedirectToAction("EmployeeView", "User");
            }

            return RedirectToAction("Index", "Home");
        }

        // Admin View - Display all users
        public IActionResult AdminView(string searchQuery)
        {
            var users = _context.Users.AsQueryable();  // Start with all users

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Filter by username or role (or other fields you want to search)
                users = users.Where(u => u.Username.Contains(searchQuery) || u.Role.Contains(searchQuery));
            }

            return View(users.ToList());
        }

        // Employee View - Add a new employee
        public IActionResult EmployeeView()
        {
            return View();
        }

        // Create New User (Admin only)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            // Check if the username already exists in the database
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(user); // Return the same view with error message
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("AdminView");
            }
            return View(user);
        }

        // Edit User (Admin only) - GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Edit User (Admin only) - POST
        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.Username = user.Username;

                // Only update the password if the new password is not null or empty
                if (!string.IsNullOrEmpty(user.Password_hash))
                {
                    existingUser.Password_hash = user.Password_hash;
                }

                existingUser.Role = user.Role;

                // Save changes to the database
                try
                {
                    _context.SaveChanges();
                    _logger.LogInformation($"User {user.Username} (ID: {user.Id}) has been successfully updated.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating user: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the changes.");
                    return View(user);
                }

                // Redirect back to AdminView after successful edit
                return RedirectToAction("AdminView");
            }

            // Return the same view if validation fails
            return View(user);
        }

        // Delete User (Admin only)
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("AdminView");
        }

        // Create Employee (Employee role only)
        [HttpPost]
        public IActionResult CreateEmployee(User user)
        {
            // Ensure the logged-in user is an Employee
            var loggedInRole = HttpContext.Session.GetString("Role");

            if (loggedInRole != "Employee")
            {
                return RedirectToAction("Index", "Home"); // Redirect if not an employee
            }

            // Automatically set the role to "Employee"
            user.Role = "Employee";

            // Check if the username already exists in the database
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View("EmployeeView", user); // Return the EmployeeView with error message
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                // Set success message using TempData to persist it through a redirect
                TempData["SuccessMessage"] = "Employee has been successfully added.";

                // Redirect to the EmployeeView after successful addition
                return RedirectToAction("EmployeeView", "User");
            }

            // If model validation fails return to the CreateEmployee view
            return View("EmployeeView", user);
        }

        // Logout 
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
