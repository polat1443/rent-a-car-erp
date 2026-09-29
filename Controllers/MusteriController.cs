using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AracKiralamaApp.Controllers
{
    public class MusteriController : Controller
    {
        private readonly AracKiralamaContext _context;

        public MusteriController(AracKiralamaContext context)
        {
            _context = context;
        }

        // GET: Musteri
        public async Task<IActionResult> Index()
        {
            var musteriler = await _context.Musteriler.ToListAsync();
            return View(musteriler);
        }

        // GET: Musteri/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Musteri/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MusteriId,AdSoyad,TcNo,EhliyetNo,Telefon,Email,RiskSkoru,Blacklist,KiralamaSikligiSegment")] Musteri musteri)
        {
            if (ModelState.IsValid)
            {
                // Check if TcNo exists
                var tcExists = await _context.Musteriler.AnyAsync(m => m.TcNo == musteri.TcNo);
                if (tcExists)
                {
                    ModelState.AddModelError("TcNo", "Bu T.C. Kimlik Numarası zaten kayıtlı.");
                    return View(musteri);
                }

                _context.Add(musteri);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Müşteri başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            return View(musteri);
        }

        // GET: Musteri/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var musteri = await _context.Musteriler.FindAsync(id);
            if (musteri == null)
            {
                return NotFound();
            }
            return View(musteri);
        }

        // POST: Musteri/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MusteriId,AdSoyad,TcNo,EhliyetNo,Telefon,Email,RiskSkoru,Blacklist,KiralamaSikligiSegment")] Musteri musteri)
        {
            if (id != musteri.MusteriId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if TcNo exists in other customers
                    var tcExists = await _context.Musteriler.AnyAsync(m => m.TcNo == musteri.TcNo && m.MusteriId != musteri.MusteriId);
                    if (tcExists)
                    {
                        ModelState.AddModelError("TcNo", "Bu T.C. Kimlik Numarası başka bir müşteride kayıtlı.");
                        return View(musteri);
                    }

                    _context.Update(musteri);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Müşteri bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MusteriExists(musteri.MusteriId))
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
            return View(musteri);
        }

        // GET: Musteri/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var musteri = await _context.Musteriler
                .FirstOrDefaultAsync(m => m.MusteriId == id);
            if (musteri == null)
            {
                return NotFound();
            }

            return View(musteri);
        }

        // POST: Musteri/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musteri = await _context.Musteriler.FindAsync(id);
            if (musteri == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Musteriler.Remove(musteri);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Müşteri başarıyla silindi.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Bu müşteri adına kayıtlı kiralama geçmişi olduğu için silinemez.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MusteriExists(int id)
        {
            return _context.Musteriler.Any(e => e.MusteriId == id);
        }
    }
}
