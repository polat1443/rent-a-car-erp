using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AracKiralamaApp.Controllers
{
    public class PersonelController : Controller
    {
        private readonly AracKiralamaContext _context;

        public PersonelController(AracKiralamaContext context)
        {
            _context = context;
        }

        // GET: Personel
        public async Task<IActionResult> Index(int? subeId)
        {
            var query = _context.Personeller.Include(p => p.Sube).AsQueryable();

            if (subeId.HasValue && subeId.Value > 0)
            {
                query = query.Where(p => p.SubeId == subeId.Value);
                ViewBag.SelectedSubeId = subeId.Value;
            }

            var personeller = await query.OrderBy(p => p.AdSoyad).ToListAsync();
            
            // Load branches for filter and add form
            var subeler = await _context.Subeler.ToListAsync();
            ViewBag.SubelerList = subeler;
            ViewBag.SubeId = new SelectList(subeler, "SubeId", "SubeAdi");

            return View(personeller);
        }

        // POST: Personel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubeId,AdSoyad,Rol,Maas,IseGirisTarihi")] Personel personel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(personel);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yeni personel kaydı başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Personel eklenirken geçerli bilgileri girdiğinizden emin olun.";
            return RedirectToAction(nameof(Index));
        }
    }
}
