using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArchiveDomain.Model;
using ArchiveInfrastructure;

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
            var dbarchiveContext = _context.Poetries.Include(p => p.Admin).Include(p => p.Author).Include(p => p.Language);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (poetry == null)
            {
                return NotFound();
            }

            return View(poetry);
        }

        // GET: Poetries/Create
        public IActionResult Create()
        {
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email");
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1");
            return View();
        }

        // POST: Poetries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorId,Title,Text,PublicationDate,LanguageId,AdminId,Id")] Poetry poetry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(poetry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            return View(poetry);
        }

        // GET: Poetries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poetry = await _context.Poetries.FindAsync(id);
            if (poetry == null)
            {
                return NotFound();
            }
            ViewData["AdminId"] = new SelectList(_context.Admins, "Id", "Email", poetry.AdminId);
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
            return View(poetry);
        }

        // POST: Poetries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuthorId,Title,Text,PublicationDate,LanguageId,AdminId,Id")] Poetry poetry)
        {
            if (id != poetry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(poetry);
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
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", poetry.AuthorId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Language1", poetry.LanguageId);
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
                .Include(p => p.Admin)
                .Include(p => p.Author)
                .Include(p => p.Language)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var poetry = await _context.Poetries.FindAsync(id);
            if (poetry != null)
            {
                _context.Poetries.Remove(poetry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PoetryExists(int id)
        {
            return _context.Poetries.Any(e => e.Id == id);
        }
    }
}
