using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArchiveDomain.Model;
using ArchiveInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ArchiveInfrastructure.Controllers
{
    [Authorize]
    public class PoetriesController : Controller
    {
        private readonly DbarchiveContext _context;
        private readonly UserManager<User> _userManager;

        public PoetriesController(DbarchiveContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Poetries
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? languageId, int? authorId, int? formId)
        {
            var poetries = _context.Poetries
                .Include(p => p.AddedByUser)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Forms)
                .AsQueryable();

            if (languageId.HasValue)
            {
                poetries = poetries.Where(p => p.LanguageId == languageId.Value);
            }

            if (authorId.HasValue)
            {
                poetries = poetries.Where(p => p.AuthorId == authorId.Value);
            }

            if (formId.HasValue)
            {
                poetries = poetries.Where(p => p.Forms.Any(f => f.Id == formId.Value));
            }

            ViewBag.LanguageId = new SelectList(_context.Languages, "Id", "Language1", languageId);
            ViewBag.AuthorId = new SelectList(
                _context.Authors.Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName }),
                "Id", "FullName", authorId);
            ViewBag.FormId = new SelectList(_context.Forms, "Id", "FormName", formId);

            return View(await poetries.ToListAsync());
        }

        // GET: Poetries/Liked
        public async Task<IActionResult> Liked()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var likedPoetries = _context.Poetries
                .Include(p => p.AddedByUser)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Forms)
                .Where(p => p.LikedByUsers.Any(u => u.Id == user.Id));

            return View(await likedPoetries.ToListAsync());
        }

        // GET: Poetries/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries
                .Include(p => p.AddedByUser)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Forms)
                .Include(p => p.LikedByUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            return View(poetry);
        }

        // GET: Poetries/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1");
            ViewData["Forms"] = new MultiSelectList(_context.Forms, "Id", "FormName");
            return View();
        }

        // POST: Poetries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AuthorId,Title,Text,LanguageId")] Poetry poetry, string? SelectedFormIds)
        {
            poetry.PublicationDate = DateTime.Now;
            poetry.AddedByUserId = _userManager.GetUserId(User);

            var author = await _context.Authors.FindAsync(poetry.AuthorId);
            var language = await _context.Languages.FindAsync(poetry.LanguageId);
            var admin = await _context.Users.FindAsync(poetry.AddedByUserId);

            poetry.AddedByUser = admin;
            poetry.Author = author;
            poetry.Language = language;

            ModelState.Clear();
            TryValidateModel(poetry);

            var existingPoetry = await _context.Poetries
                .FirstOrDefaultAsync(p => p.Title == poetry.Title && p.AuthorId == poetry.AuthorId && p.LanguageId == poetry.LanguageId);
            if (existingPoetry != null)
            {
                ModelState.AddModelError(string.Empty, "Поезія з такою назвою, автором та мовою вже існує.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(poetry);

                if (!string.IsNullOrEmpty(SelectedFormIds))
                {
                    var formIds = SelectedFormIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(int.Parse)
                        .ToList();

                    foreach (var formId in formIds)
                    {
                        var form = await _context.Forms.FindAsync(formId);
                        if (form != null)
                        {
                            poetry.Forms.Add(form);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Поезію успішно додано.";
                return RedirectToAction(nameof(Index));
            }

            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new MultiSelectList(_context.Forms, "Id", "FormName");
            return View(poetry);
        }

        // GET: Poetries/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Forms)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new MultiSelectList(_context.Forms, "Id", "FormName", poetry.Forms.Select(f => f.Id));
            return View(poetry);
        }

        // POST: Poetries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AuthorId,Title,Text,LanguageId")] Poetry poetry, string? SelectedFormIds)
        {
            if (id != poetry.Id)
            {
                return NotFound();
            }

            var poetryToUpdate = await _context.Poetries
                .Include(p => p.Forms)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.AddedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poetryToUpdate == null)
            {
                return NotFound();
            }

            // Оновлення основних полів
            poetryToUpdate.AuthorId = poetry.AuthorId;
            poetryToUpdate.Title = poetry.Title;
            poetryToUpdate.Text = poetry.Text;
            poetryToUpdate.LanguageId = poetry.LanguageId;

            // Збереження AddedByUserId (залишаємо оригінальне значення з бази)
            // Якщо потрібно оновити до поточного адміна, можна додати:
            // poetryToUpdate.AddedByUserId = _userManager.GetUserId(User);

            // Перевірка унікальності
            var existingPoetry = await _context.Poetries
                .FirstOrDefaultAsync(p => p.Title == poetry.Title && p.AuthorId == poetry.AuthorId && p.LanguageId == poetry.LanguageId && p.Id != id);
            if (existingPoetry != null)
            {
                ModelState.AddModelError(string.Empty, "Поезія з такою назвою, автором та мовою вже існує.");
            }

            // Очищення ModelState і валідація
            ModelState.Clear();
            TryValidateModel(poetryToUpdate);

            if (ModelState.IsValid)
            {
                // Оновлення форм
                poetryToUpdate.Forms.Clear();
                if (!string.IsNullOrEmpty(SelectedFormIds))
                {
                    var formIds = SelectedFormIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(int.Parse)
                        .ToList();

                    foreach (var formId in formIds)
                    {
                        var form = await _context.Forms.FindAsync(formId);
                        if (form != null)
                        {
                            poetryToUpdate.Forms.Add(form);
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Поезію успішно оновлено.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PoetryExists(poetry.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // Повернення при помилці
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new MultiSelectList(_context.Forms, "Id", "FormName", poetryToUpdate.Forms.Select(f => f.Id));
            return View(poetry);
        }

        // GET: Poetries/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries
                .Include(p => p.AddedByUser)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Forms)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            return View(poetry);
        }

        // POST: Poetries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var poetry = await _context.Poetries
                .Include(p => p.Forms)
                .Include(p => p.LikedByUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            _context.Poetries.Remove(poetry);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Поезію успішно видалено.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Poetries/Like/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var poetry = await _context.Poetries
                .Include(p => p.LikedByUsers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (poetry.LikedByUsers.Contains(user))
            {
                poetry.LikedByUsers.Remove(user);
                TempData["SuccessMessage"] = "Поезію видалено з обраного.";
            }
            else
            {
                poetry.LikedByUsers.Add(user);
                TempData["SuccessMessage"] = "Поезію додано до обраного.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Poetries/SearchAuthors
        [HttpGet]
        public async Task<IActionResult> SearchAuthors(string term)
        {
            var authors = await _context.Authors
                .Where(a => a.FirstName.Contains(term) || a.LastName.Contains(term))
                .Select(a => new { id = a.Id, text = a.FirstName + " " + a.LastName })
                .Take(10)
                .ToListAsync();

            return Json(authors);
        }

        private bool PoetryExists(int id)
        {
            return _context.Poetries.Any(e => e.Id == id);
        }
    }
}