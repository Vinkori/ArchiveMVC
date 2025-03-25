using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArchiveDomain.Model;
using ArchiveInfrastructure;
using static System.Net.Mime.MediaTypeNames;

namespace ArchiveInfrastructure.Controllers
{
    public class PoetriesController : Controller
    {
        private readonly DbarchiveContext _context;

        public PoetriesController(DbarchiveContext context)
        {
            _context = context;
        }

        // GET: Poetries
        public async Task<IActionResult> Index()
        {
            var poetries = await _context.Poetries
                                            .Include(p => p.Admin)
                                            .Include(p => p.Author)
                                            .Include(p => p.Language)
                                            .Include(p => p.Forms)
                                            .ToListAsync();
            return View(poetries);

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

            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            //ViewData["Authors"] = new SelectList(_context.Authors, "Id", "Name");

            return View(poetry);
        }

        // POST: Poetries/RemoveFormFromPoetry
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Poetries/RemoveFormFromPoetry")]
        public async Task<IActionResult> RemoveFormFromPoetry(int poetryId, int formId)
        {
            var poetry = await _context.Poetries
                                        .Include(p => p.Forms)
                                        .FirstOrDefaultAsync(p => p.Id == poetryId);

            if (poetry == null)
            {
                return NotFound();
            }

            var form = poetry.Forms.FirstOrDefault(g => g.Id == formId);
            if (form != null)
            {
                poetry.Forms.Remove(form);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = poetryId });
        }

        // POST: Poetries/AddFormToPoetry
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Poetries/AddFormToPoetry")]
        public async Task<IActionResult> AddFormToPoetry(int id, int[] selectedForms)
        {
            var poetry = await _context.Poetries
                                        .Include(p => p.Forms)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            if (selectedForms != null)
            {
                foreach (var formId in selectedForms)
                {
                    var form = await _context.Forms.FindAsync(formId);
                    if (form != null && !poetry.Forms.Contains(form))
                    {
                        poetry.Forms.Add(form);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = poetry.Id });
        }

        // GET: Poetries/Create
        public IActionResult Create()
        {
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email");
            // Формуємо комбінований список авторів (FirstName + LastName)
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1");
            // Передаємо список жанрів (Forms)
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            var readers = _context.Readers
                .Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName })
                .ToList();
            ViewData["Readers"] = new SelectList(readers, "Id", "FullName");
            return View();
        }

        // POST: Poetries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Видаляємо PublicationDate із Bind, адже вона задається автоматично
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        [Bind("AuthorId,Title,Text,LanguageId,AdminId,Id")] Poetry poetry,
        int[] selectedFormIds,   
        int[] selectedReaderIds)
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
            var admin = await _context.Admins.FindAsync(poetry.AdminId);

            poetry.Author = author;
            poetry.Language = language;
            poetry.Admin = admin;

            ModelState.Clear();
            TryValidateModel(poetry);

            var existingProduct = await _context.Poetries 
                                                .Where(p => p.Title == poetry.Title && p.AuthorId == poetry.AuthorId && p.LanguageId == poetry.LanguageId)
                                                .FirstOrDefaultAsync();
            if (existingProduct != null)
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
                if (selectedFormIds != null)
                {
                    foreach (var formId in selectedFormIds)
                    {
                        var form = await _context.Forms.FindAsync(formId);
                        if (form != null)
                        {
                            poetry.Forms.Add(form);
                        }
                    }
                }
                
                // Обробка вибраних читачів (Readers, лайки)
                if (selectedReaderIds != null)
                {
                    foreach (var readerId in selectedReaderIds)
                    {
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

            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            var readers = _context.Readers
        .Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName })
        .ToList();
            ViewData["Readers"] = new SelectList(readers, "Id", "FullName");
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
                .Include(p => p.Forms)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (poetry == null)
            {
                return NotFound();
            }
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            var authors = _context.Authors
                .Select(a => new { a.Id, FullName = a.FirstName + " " + a.LastName })
                .ToList();
            ViewData["AuthorId"] = new SelectList(authors, "Id", "FullName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            // Передаємо список жанрів (Forms) для вибору
            ViewData["Forms"] = new SelectList(_context.Forms, "Id", "FormName");
            return View(poetry);
        }

        // POST: Poetries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        [Bind("AuthorId,Title,Text,LanguageId,AdminId,Id")] Poetry poetry,
        string? SelectedFormIds) // рядок з комою розділеними ID жанрів
        {
            if (id != poetry.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Author");
            ModelState.Remove("Language");
            ModelState.Remove("Admin");
            ModelState.Remove("Form");

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
                poetryToUpdate.AdminId = poetry.AdminId;

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
                .Include(p => p.Author)
                .Include(p => p.Admin)
                .Include(p => p.Language)
                .Include(p => p.Readers)
                .Include(p => p.Forms) // Включаємо жанри, щоб потім видалити зв'язки
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
                .Include(p => p.Author)
                .Include(p => p.Admin)
                .Include(p => p.Language)
                .Include(p => p.Readers)
                .Include(p => p.Forms)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (poetry == null)
            {
                return NotFound();
            }

            // Видаляємо зв'язки з жанрами, щоб уникнути проблем
            poetry.Forms.Clear();
            poetry.Readers.Clear();

            _context.Poetries.Remove(poetry);
            await _context.SaveChangesAsync(); // Остаточно видаляємо поезію

            TempData["SuccessMessage"] = "Поезію було успішно видалено.";
            return RedirectToAction(nameof(Index));
        }

        private bool PoetryExists(int id)
        {
            return _context.Poetries.Any(e => e.Id == id);
        }
    }
}
