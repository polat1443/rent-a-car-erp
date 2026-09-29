using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AracKiralamaApp.Controllers
{
    public class AracController : Controller
    {
        private readonly AracKiralamaContext _context;

        public AracController(AracKiralamaContext context)
        {
            _context = context;
        }

        // GET: Arac
        public async Task<IActionResult> Index()
        {
            var araclar = await _context.Araclar.Include(a => a.BulunduguSube).ToListAsync();
            return View(araclar);
        }

        // GET: Arac/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi");
            return View();
        }

        // POST: Arac/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AracId,Plaka,Marka,Model,Yil,GunlukFiyat,Durum,SatinAlmaBedeli,BulunduguSubeId")] Arac arac)
        {
            if (ModelState.IsValid)
            {
                var plakaExists = await _context.Araclar.AnyAsync(a => a.Plaka == arac.Plaka);
                if (plakaExists)
                {
                    ModelState.AddModelError("Plaka", "Bu plaka zaten kayıtlı.");
                    ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi", arac.BulunduguSubeId);
                    return View(arac);
                }

                _context.Add(arac);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Araç başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi", arac.BulunduguSubeId);
            return View(arac);
        }

        // GET: Arac/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arac = await _context.Araclar.FindAsync(id);
            if (arac == null)
            {
                return NotFound();
            }
            ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi", arac.BulunduguSubeId);
            return View(arac);
        }

        // POST: Arac/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AracId,Plaka,Marka,Model,Yil,GunlukFiyat,Durum,SatinAlmaBedeli,BulunduguSubeId")] Arac arac)
        {
            if (id != arac.AracId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var plakaExists = await _context.Araclar.AnyAsync(a => a.Plaka == arac.Plaka && a.AracId != arac.AracId);
                    if (plakaExists)
                    {
                        ModelState.AddModelError("Plaka", "Bu plaka başka bir araçta kayıtlı.");
                        ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi", arac.BulunduguSubeId);
                        return View(arac);
                    }

                    _context.Update(arac);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Araç bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AracExists(arac.AracId))
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
            ViewBag.BulunduguSubeId = new SelectList(await _context.Subeler.ToListAsync(), "SubeId", "SubeAdi", arac.BulunduguSubeId);
            return View(arac);
        }

        // GET: Arac/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arac = await _context.Araclar
                .Include(a => a.BulunduguSube)
                .FirstOrDefaultAsync(m => m.AracId == id);
            if (arac == null)
            {
                return NotFound();
            }

            return View(arac);
        }

        // POST: Arac/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var arac = await _context.Araclar.FindAsync(id);
            if (arac == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Araclar.Remove(arac);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Araç başarıyla silindi.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Bu araç aktif veya geçmiş bir kiralama işleminde kullanıldığı için silinemez. Durumunu 'Bakimda' olarak güncellemeyi deneyebilirsiniz.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AracExists(int id)
        {
            return _context.Araclar.Any(e => e.AracId == id);
        }
    }
}
