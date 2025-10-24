using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.ViewModels
[HttpPost]
public IActionResult Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Example authentication logic (replace with your actual logic)
    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

    if (user == null)
    {
        ModelState.AddModelError(string.Empty, "This account does not exist.");
        return View(model);
    }

    if (user.Password != model.Password) // or use password hash verification
    {
        ModelState.AddModelError(string.Empty, "The username doesn't match the password.");
        return View(model);
    }

    // If login successful
    // (Set session, redirect, etc.)
    return RedirectToAction("UserHome", "Account");
}

