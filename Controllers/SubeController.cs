using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AracKiralamaApp.Controllers
{
    public class SubeController : Controller
    {
        private readonly AracKiralamaContext _context;

        public SubeController(AracKiralamaContext context)
        {
            _context = context;
        }

        // GET: Sube
        public async Task<IActionResult> Index()
        {
            var subeler = await _context.Subeler
                .Include(s => s.Personeller)
                .Include(s => s.Araclar)
                .OrderBy(s => s.SubeAdi)
                .ToListAsync();

            return View(subeler);
        }

        // POST: Sube/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubeAdi,Sehir,AylikKiraGideri,AcilisTarihi")] Sube sube)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sube);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yeni şube başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Şube eklenirken geçerli bilgileri girdiğinizden emin olun.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Sube/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sube = await _context.Subeler
                .Include(s => s.Personeller)
                .Include(s => s.Araclar)
                .FirstOrDefaultAsync(m => m.SubeId == id);

            if (sube == null)
            {
                return NotFound();
            }

            // Calculate branch dynamic opex: monthly rent + sum of staff salaries
            decimal toplamPersonelMaas = sube.Personeller?.Sum(p => p.Maas) ?? 0;
            ViewBag.ToplamAylikGider = sube.AylikKiraGideri + toplamPersonelMaas;
            ViewBag.ToplamPersonelMaas = toplamPersonelMaas;

            return View(sube);
        }
    }
}
