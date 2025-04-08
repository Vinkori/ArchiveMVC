using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ArchiveInfrastructure.Models;
using ArchiveInfrastructure.ViewModels;
using ArchiveInfrastructure.Models;
using ArchiveInfrastructure.ViewModels;
using ArchiveInfrastructure;

namespace ArchiveInfrastructure.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Name = model.Name // <-- додали поле Name
            };
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ця електронна пошта вже використовується.");
                return View(model); // <-- правильний варіант для MVC
            }
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Reader");
                //return RedirectToAction("Login"); // Redirect to Login page
                TempData["SuccessMessage"] = "Реєстрація пройшла успішно. Ласкаво просимо!";
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                // Log error messages for further investigation
                Console.WriteLine($"Registration Error: {error.Description}"); // Or use a logging framework
                TempData["ErrorMessage"] = "Помилка реєстрації: " + error.Description;
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl // Set the ReturnUrl in the ViewModel
            };
            return View(model);

        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Вхід виконано успішно!";
                // перевіряємо, чи належить URL додатку
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else
                {

                    return RedirectToAction("Index", "Home"); // Redirect after success
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Неправильний логін або пароль.";
                ModelState.AddModelError("", "Неправильний логін чи (та) пароль");
            }


            return View(model);
        }

        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                UserName = user.UserName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // Перевірка унікальності username
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null && existingUser.Id != model.Id)
            {
                ModelState.AddModelError("UserName", "Такий ім’я користувача вже існує.");
                return View(model);
            }

            user.UserName = model.UserName;
            user.Name = model.Name;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }

}