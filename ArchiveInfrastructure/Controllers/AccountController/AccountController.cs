using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ArchiveDomain.Model;
using ArchiveInfrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ArchiveInfrastructure.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Ініціалізація ролей
        private async Task InitializeRoles()
        {
            string[] roles = { "Admin", "Reader" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            await InitializeRoles();

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
                Name = model.Name
            };

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ця електронна пошта вже використовується.");
                return View(model);
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Reader");
                await _signInManager.SignInAsync(user, false);
                TempData["SuccessMessage"] = "Реєстрація пройшла успішно!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Вхід виконано успішно!";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Неправильний логін або пароль.";
            ModelState.AddModelError("", "Неправильний логін чи пароль");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Ви вийшли з системи.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/CreateAdmin
        [HttpGet]
        public async Task<IActionResult> CreateAdmin()
        {
            await InitializeRoles();
            var admin = await _userManager.FindByEmailAsync("admin@example.com");
            if (admin == null)
            {
                var user = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    Name = "Адміністратор"
                };
                var result = await _userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    return Content("Адміністратора створено.");
                }
                return Content("Помилка: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return Content("Адміністратор уже існує.");
        }

        // GET: /Account/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Name = user.Name
            };

            return View(model);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null || user.Id != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            // Оновлення особистих даних
            if (user.UserName != model.UserName)
            {
                var userNameExists = await _userManager.FindByNameAsync(model.UserName);
                if (userNameExists != null && userNameExists.Id != user.Id)
                {
                    ModelState.AddModelError("UserName", "Це ім’я користувача вже зайняте.");
                    return View(model);
                }
                user.UserName = model.UserName;
            }

            user.Name = model.Name;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Оновлення сесії
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Профіль успішно оновлено.";
            return RedirectToAction(nameof(EditProfile));
        }
    }
}