using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArchiveDomain.Model;
using ArchiveInfrastructure;
using Microsoft.AspNetCore.Authorization;
using ArchiveInfrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace ArchiveInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Index()
        {
            var dbarchiveContext = _context.Poetries.Include(p => p.Admin).Include(p => p.Author).Include(p => p.Language).Include(p=>p.Forms);
            return View(await dbarchiveContext.ToListAsync());

        }

        // GET: Poetries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries
                .Include(p => p.Admin)
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

        // GET: Poetries/Create
        public async Task<IActionResult> Create()
        {
            //ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email");
            // Формуємо комбінований список авторів (FirstName + LastName)
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1");
            // Передаємо список жанрів (Forms)
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            // Отримуємо список користувачів із роллю "Reader"
            var readers = await _userManager.GetUsersInRoleAsync("Reader");
            var readerList = readers.Select(r => new
            {
                r.Id,
                FullName = string.IsNullOrEmpty(r.Name) ? r.UserName : r.Name
            }).ToList();
            ViewData["Readers"] = new SelectList(readerList, "Id", "FullName");
            return View();
        }

        // POST: Poetries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Видаляємо PublicationDate із Bind, адже вона задається автоматично
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        [Bind("AuthorId,Title,Text,LanguageId,Id")] Poetry poetry,
        string? SelectedFormIds,   // Рядок з комою розділеними ID жанрів
        string? SelectedReaderIds)
        {
            // Встановлюємо дату публікації згідно з поточним місцевим часом
            poetry.PublicationDate = DateTime.Now;

            /*
            // Видаляємо неактуальні властивості для перевірки моделі
            ModelState.Remove("Author");
            ModelState.Remove("Language");
            ModelState.Remove("Admin");
            ModelState.Remove("Readers");*/
            var author = await _context.Authors.FindAsync(poetry.AuthorId);
            var language = await _context.Languages.FindAsync(poetry.LanguageId);
            //var admin = await _context.Admins.FindAsync(poetry.AdminId);

            poetry.Author = author;
            poetry.Language = language;

            var currentAdmin = await _userManager.GetUserAsync(User);
            poetry.Admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == currentAdmin.Email);

            ModelState.Clear();
            TryValidateModel(poetry);

            var existingPoetry = await _context.Poetries
                                                .Where(p => p.Title == poetry.Title && p.AuthorId == poetry.AuthorId && p.LanguageId == poetry.LanguageId)
                                                .FirstOrDefaultAsync();
            if (existingPoetry != null)
            {
                ModelState.AddModelError(string.Empty, "Поезія з такою назвою, автором та такою мовою вже існує.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(poetry);
                // Обробка вибраних жанрів
                if (!string.IsNullOrEmpty(SelectedFormIds))
                {
                    var formIds = SelectedFormIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => int.Parse(id))
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

                if (!string.IsNullOrEmpty(SelectedReaderIds))
                {
                    var readerIds = SelectedReaderIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => int.Parse(id))
                        .ToList();

                    foreach (var readerId in readerIds)
                    {
                        // Оновлено: використовуємо _context.Readers, а не _context.Forms
                        var reader = await _context.Readers.FindAsync(readerId);
                        if (reader != null)
                        {
                            poetry.Readers.Add(reader);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            //ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            var readers = await _userManager.GetUsersInRoleAsync("Reader");
            var readerList = readers.Select(r => new { r.Id, FullName = string.IsNullOrEmpty(r.Name) ? r.UserName : r.Name }).ToList();
            ViewData["Readers"] = new SelectList(readerList, "Id", "FullName");
            return View(poetry);
        }

        // GET: Poetries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Включаємо жанри для існуючої поезії
            var poetry = await _context.Poetries
                .Include(p => p.Admin)
                .Include(p => p.Author)
                .Include(p => p.Language)
                .Include(p => p.Readers)
                .Include(p => p.Forms)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (poetry == null)
            {
                return NotFound();
            }
            //ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            // Передаємо список жанрів (Forms) для вибору
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            var readers = await _userManager.GetUsersInRoleAsync("Reader");
            var readerList = readers.Select(r => new { r.Id, FullName = string.IsNullOrEmpty(r.Name) ? r.UserName : r.Name }).ToList();
            ViewData["Readers"] = new SelectList(readerList, "Id", "FullName");
            return View(poetry);
        }

        // POST: Poetries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        [Bind("AuthorId,Title,Text,LanguageId,Id")] Poetry poetry,
        string SelectedFormIds,   // Рядок з комою розділеними ID жанрів
        string? SelectedReaderIds)
        {
            if (id != poetry.Id)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(poetry.AuthorId);
            var language = await _context.Languages.FindAsync(poetry.LanguageId);
            //var admin = await _context.Admins.FindAsync(poetry.AdminId);

            poetry.Author = author;
            poetry.Language = language;
            //poetry.Admin = admin;
            var currentAdmin = await _userManager.GetUserAsync(User);
            poetry.Admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == currentAdmin.Email);


            ModelState.Clear();
            TryValidateModel(poetry);

            var existingPoetry = await _context.Poetries
                                                .Where(p => p.Title == poetry.Title && p.AuthorId == poetry.AuthorId && p.LanguageId == poetry.LanguageId)
                                                .FirstOrDefaultAsync();
            if (existingPoetry != null)
            {
                ModelState.AddModelError(string.Empty, "Поезія з такою назвою, автором та такою мовою вже існує.");
            }

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var err in allErrors)
                {
                    Console.WriteLine(err);
                }
                // ... повернути форму або робити щось іще
            }

            if (ModelState.IsValid)
            {
                // Завантажуємо існуючу поезію з жанрами
                var poetryToUpdate = await _context.Poetries
                    .Include(p => p.Forms)
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (poetryToUpdate == null)
                {
                    return NotFound();
                }

                // Оновлюємо змінні властивості (PublicationDate залишаємо незмінною)
                poetryToUpdate.AuthorId = poetry.AuthorId;
                poetryToUpdate.Title = poetry.Title;
                poetryToUpdate.Text = poetry.Text;
                poetryToUpdate.LanguageId = poetry.LanguageId;
                //poetryToUpdate.AdminId = poetry.AdminId;

                // Оновлюємо зв'язок з жанрами: очищуємо та додаємо вибрані
                poetryToUpdate.Forms.Clear();
                if (!string.IsNullOrEmpty(SelectedFormIds))
                {
                    var formIds = SelectedFormIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => int.Parse(id))
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

                poetryToUpdate.Readers.Clear();
                if (!string.IsNullOrEmpty(SelectedReaderIds))
                {
                    var readerIds = SelectedReaderIds.Split(',')
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => int.Parse(id))
                        .ToList();

                    foreach (var readerId in readerIds)
                    {
                        var reader = await _context.Forms.FindAsync(readerId);
                        if (reader != null)
                        {
                            poetryToUpdate.Forms.Add(reader);
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PoetryExists(poetry.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            var readers = await _userManager.GetUsersInRoleAsync("Reader");
            var readerList = readers.Select(r => new { r.Id, FullName = string.IsNullOrEmpty(r.Name) ? r.UserName : r.Name }).ToList();
            ViewData["Readers"] = new SelectList(readerList, "Id", "FullName");
            return View(poetry);
        }


        // GET: Poetries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries
                .Include(p => p.Forms) // Включаємо жанри, щоб потім видалити зв'язки
                .Include(p => p.Readers)
                .Include(p => p.Author)
                .Include(p => p.Admin)
                .Include(p => p.Language)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            return View(poetry); // Відображаємо сторінку підтвердження
        }

        // POST: Poetries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var poetry = await _context.Poetries
        .Include(p => p.Forms) // Завантажуємо пов'язану колекцію жанрів
        .Include(p => p.Readers)
        .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            // Видаляємо зв'язки з жанрами, щоб уникнути проблем
            poetry.Forms.Clear();
            poetry.Readers.Clear();
            await _context.SaveChangesAsync(); // Спочатку оновлюємо стан БД
            TempData["SuccessMessage"] = "Поезію було успішно видалено.";
            _context.Poetries.Remove(poetry);
            await _context.SaveChangesAsync(); // Остаточно видаляємо поезію

            return RedirectToAction(nameof(Index));
        }

        private bool PoetryExists(int id)
        {
            return _context.Poetries.Any(e => e.Id == id);
        }
    }
}
