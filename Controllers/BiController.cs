using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using AracKiralamaApp.Services;
using AracKiralamaApp.Infrastructure.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AracKiralamaApp.Controllers
{
    public class BiController : Controller
    {
        private readonly AracKiralamaContext _context;
        private readonly MLService _mlService;

        public BiController(AracKiralamaContext context, MLService mlService)
        {
            _context = context;
            _mlService = mlService;
        }

        // GET: Bi/Index
        public async Task<IActionResult> Index()
        {
            // Load vehicles with rental history to display calculated ROI
            var vehicles = await _context.Araclar
                .Include(a => a.Kiralamalar)
                .ToListAsync();

            // Load customers with segment and risk score details
            var customers = await _context.Musteriler
                .Include(m => m.Kiralamalar)
                .OrderByDescending(m => m.RiskSkoru)
                .ToListAsync();

            // Load latest system logs
            var logs = await _context.SystemLogs
                .OrderByDescending(l => l.LogTarihi)
                .Take(20)
                .ToListAsync();

            // Prepare list of vehicles for KNN Dropdown selector
            ViewBag.AracList = await _context.Araclar
                .Select(a => new { Id = a.AracId, Text = $"{a.Plaka} - {a.Marka} {a.Model} (Baz: {a.GunlukFiyat:C})" })
                .ToListAsync();

            return View(new BiViewModel
            {
                Araclar = vehicles,
                Musteriler = customers,
                SystemLogs = logs
            });
        }

        // POST: Bi/TriggerKMeans
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerKMeans()
        {
            try
            {
                await _mlService.RunKMeansSegmentationAsync();
                TempData["SuccessMessage"] = "K-Means Kümeleme algoritması başarıyla çalıştırıldı ve müşteri segmentleri güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"K-Means çalıştırılırken hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bi/OptimizePriceKNN
        [HttpGet]
        public async Task<IActionResult> OptimizePriceKNN(int aracId, int year, int month, int duration)
        {
            if (aracId <= 0) return BadRequest("Geçersiz araç seçimi.");
            
            var recommendedPrice = await _mlService.OptimizePriceKNNAsync(aracId, year, month, duration);
            var arac = await _context.Araclar.FindAsync(aracId);

            return Json(new
            {
                success = true,
                basePrice = arac?.GunlukFiyat ?? 0,
                recommendedPrice = recommendedPrice,
                message = $"KNN algoritması (K=3) ile önerilen günlük fiyat: {recommendedPrice:C}"
            });
        }

        // GET: Bi/DecryptLog/5
        [HttpGet]
        public async Task<IActionResult> DecryptLog(int id)
        {
            var log = await _context.SystemLogs.FindAsync(id);
            if (log == null) return NotFound("Log kaydı bulunamadı.");

            try
            {
                var decryptedEski = EncryptionHelper.Decrypt(log.EskiVeri);
                var decryptedYeni = EncryptionHelper.Decrypt(log.YeniVeri);

                return Json(new
                {
                    success = true,
                    eskiVeri = decryptedEski,
                    yeniVeri = decryptedYeni
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Deşifre hatası: {ex.Message}" });
            }
        }

        // POST: Bi/ToggleBlacklist/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlacklist(int id)
        {
            var customer = await _context.Musteriler.FindAsync(id);
            if (customer == null) return NotFound();

            customer.Blacklist = !customer.Blacklist;
            
            // Eğer kara listeye alındıysa risk skorunu 100 yap, çıkarıldıysa 0 yap
            customer.RiskSkoru = customer.Blacklist ? 100 : 0;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Müşteri ({customer.AdSoyad}) kara liste durumu güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class BiViewModel
    {
        public List<Arac> Araclar { get; set; } = new();
        public List<Musteri> Musteriler { get; set; } = new();
        public List<SystemLog> SystemLogs { get; set; } = new();
    }
}
