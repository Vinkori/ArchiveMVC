using ArchiveDomain.Model;
using ArchiveInfrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Roles
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(_roleManager.Roles.ToList());
        }

        // GET: Roles/UserList
        public IActionResult UserList()
        {
            return View(_userManager.Users.ToList());
        }

        // GET: Roles/Edit/{userId}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Користувача не вказано.";
                return RedirectToAction(nameof(UserList));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(UserList));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();
            var model = new ChangeRoleViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = user.Name ?? user.UserName,
                UserRoles = userRoles,
                AllRoles = allRoles
            };

            return View(model);
        }

        // POST: Roles/Edit/{userId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Користувача не вказано.";
                return RedirectToAction(nameof(UserList));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(UserList));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var addedRoles = roles.Except(userRoles).ToList();
            var removedRoles = userRoles.Except(roles).ToList();

            try
            {
                if (addedRoles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, addedRoles);
                    if (!addResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Помилка при додаванні ролей: " + string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return RedirectToAction(nameof(Edit), new { userId });
                    }
                }

                if (removedRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    if (!removeResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Помилка при видаленні ролей: " + string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        return RedirectToAction(nameof(Edit), new { userId });
                    }
                }

                TempData["SuccessMessage"] = "Ролі користувача успішно оновлено.";
                return RedirectToAction(nameof(UserList));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Помилка при оновленні ролей: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { userId });
            }
        }

        // GET: Roles/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Назва ролі не може бути порожньою.";
                return View();
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                TempData["ErrorMessage"] = $"Роль '{roleName}' уже існує.";
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Роль '{roleName}' успішно створено.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Помилка при створенні ролі: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        // POST: Roles/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Роль не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Роль '{role.Name}' успішно видалено.";
            }
            else
            {
                TempData["ErrorMessage"] = "Помилка при видаленні ролі: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}