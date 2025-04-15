using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArchiveInfrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiveInfrastructure.Controllers
{
    public class ChartsController : Controller
    {
        private readonly DbarchiveContext _context;

        public ChartsController(DbarchiveContext context)
        {
            _context = context;
        }

        // GET: /Charts
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Charts/GetTopPoemsByLikes
        [HttpGet]
        public async Task<IActionResult> GetTopPoemsByLikes()
        {
            var topPoems = await _context.Poetries
                .Include(p => p.LikedByUsers)
                .OrderByDescending(p => p.LikedByUsers.Count)
                .Take(5)
                .Select(p => new
                {
                    Title = p.Title,
                    Likes = p.LikedByUsers.Count
                })
                .ToListAsync();

            return Json(topPoems);
        }

        // GET: /Charts/GetLanguageDistribution
        [HttpGet]
        public async Task<IActionResult> GetLanguageDistribution()
        {
            var totalPoems = await _context.Poetries.CountAsync();

            var languageDistribution = await _context.Poetries
                .Include(p => p.Language)
                .GroupBy(p => p.Language.Language1)
                .Select(g => new
                {
                    Language = g.Key,
                    Count = g.Count(),
                    Percentage = totalPoems > 0 ? (double)g.Count() / totalPoems * 100 : 0
                })
                .ToListAsync();

            return Json(languageDistribution);
        }
    }
}