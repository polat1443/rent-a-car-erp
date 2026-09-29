using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;
using AracKiralamaApp.Infrastructure.Helpers;
using AracKiralamaApp.Infrastructure.EventBus;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AracKiralamaApp.Controllers
{
    public class KiralamaController : Controller
    {
        private readonly AracKiralamaContext _context;
        private readonly IWebHostEnvironment _env;

        public KiralamaController(AracKiralamaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Kiralama
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Kiralamalar
                .Include(k => k.Arac)
                .Include(k => k.Musteri)
                .Include(k => k.CikisSube)
                .Include(k => k.DonusSube)
                .Include(k => k.IsleyenPersonel)
                .OrderByDescending(k => k.KiralamaId)
                .ToListAsync();

            return View(rentals);
        }

        // GET: Kiralama/Create
        public async Task<IActionResult> Create()
        {
            var musteriler = await _context.Musteriler
                .Select(m => new { Id = m.MusteriId, Display = $"{m.AdSoyad} (TC: {m.TcNo})" })
                .ToListAsync();

            var subeler = await _context.Subeler
                .Select(s => new { Id = s.SubeId, Display = s.SubeAdi })
                .ToListAsync();

            var personeller = await _context.Personeller
                .Select(p => new { Id = p.PersonelId, Display = $"{p.AdSoyad} ({p.Rol})" })
                .ToListAsync();

            ViewData["MusteriId"] = new SelectList(musteriler, "Id", "Display");
            ViewData["CikisSubeId"] = new SelectList(subeler, "Id", "Display");
            ViewData["DonusSubeId"] = new SelectList(subeler, "Id", "Display");
            ViewData["IsleyenPersonelId"] = new SelectList(personeller, "Id", "Display");

            // Empty select list for vehicles initially (loaded dynamically based on CikisSube selection)
            ViewData["AracId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(new Kiralama());
        }

        // POST: Kiralama/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AracId,MusteriId,CikisSubeId,DonusSubeId,IsleyenPersonelId,BaslangicTarihi,BitisTarihi,AlisKm,LimitKm")] Kiralama kiralama)
        {
            if (kiralama.BitisTarihi < kiralama.BaslangicTarihi)
            {
                ModelState.AddModelError("BitisTarihi", "Bitiş tarihi, başlangıç tarihinden önce olamaz.");
            }

            var customer = await _context.Musteriler
                .FirstOrDefaultAsync(m => m.MusteriId == kiralama.MusteriId);

            if (customer != null && customer.Blacklist)
            {
                ModelState.AddModelError("MusteriId", "Bu müşteri kara listede olduğu için kiralama yapılamaz!");
            }

            if (ModelState.IsValid)
            {
                var vehicle = await _context.Araclar.FindAsync(kiralama.AracId);
                if (vehicle == null || vehicle.Durum != "Bos" || vehicle.BulunduguSubeId != kiralama.CikisSubeId)
                {
                    ModelState.AddModelError("AracId", "Seçilen araç bu şubede kiralanmak üzere müsait değil.");
                }
                else
                {
                    int rentalDays = (kiralama.BitisTarihi - kiralama.BaslangicTarihi).Days;
                    if (rentalDays <= 0) rentalDays = 1;

                    decimal basePrice = rentalDays * vehicle.GunlukFiyat;

                    kiralama.ToplamTutar = basePrice;
                    kiralama.KmAsimUcretiPerKm = 5.00m;
                    kiralama.GecikmeUcretiPerGun = 500.00m;
                    kiralama.TeslimEdildiMi = false;

                    // Update vehicle state to "Kirada"
                    vehicle.Durum = "Kirada";
                    _context.Update(vehicle);

                    _context.Add(kiralama);
                    await _context.SaveChangesAsync();

                    // Generate PDF contract using PdfHelper
                    var fullPath = _env.WebRootPath;
                    var pdfPath = PdfHelper.GenerateContractPdf(kiralama, fullPath);
                    kiralama.SozlesmePdfYolu = pdfPath;

                    _context.Update(kiralama);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Kiralama kaydı oluşturuldu ve sözleşme belgesi hazırlandı. Tutar: {kiralama.ToplamTutar:C}";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Reload dropdowns on validation error
            var musteriler = await _context.Musteriler
                .Select(m => new { Id = m.MusteriId, Display = m.AdSoyad })
                .ToListAsync();

            var subeler = await _context.Subeler.ToListAsync();
            var personeller = await _context.Personeller.ToListAsync();

            var eligibleVehicles = await _context.Araclar
                .Where(a => a.BulunduguSubeId == kiralama.CikisSubeId && (a.Durum == "Bos" || a.AracId == kiralama.AracId))
                .Select(a => new { Id = a.AracId, Display = $"{a.Marka} {a.Model} ({a.Plaka})" })
                .ToListAsync();

            ViewData["MusteriId"] = new SelectList(musteriler, "Id", "Display", kiralama.MusteriId);
            ViewData["CikisSubeId"] = new SelectList(subeler, "SubeId", "SubeAdi", kiralama.CikisSubeId);
            ViewData["DonusSubeId"] = new SelectList(subeler, "SubeId", "SubeAdi", kiralama.DonusSubeId);
            ViewData["IsleyenPersonelId"] = new SelectList(personeller, "PersonelId", "AdSoyad", kiralama.IsleyenPersonelId);
            ViewData["AracId"] = new SelectList(eligibleVehicles, "Id", "Display", kiralama.AracId);

            return View(kiralama);
        }

        // GET: Kiralama/GetAvailableVehicles
        [HttpGet]
        public async Task<IActionResult> GetAvailableVehicles(int subeId)
        {
            var vehicles = await _context.Araclar
                .Where(a => a.BulunduguSubeId == subeId && a.Durum == "Bos")
                .Select(a => new
                {
                    id = a.AracId,
                    display = $"{a.Marka} {a.Model} ({a.Plaka}) - {a.GunlukFiyat:C}/Gün"
                })
                .ToListAsync();

            return Json(vehicles);
        }

        // GET: Kiralama/GetPersonellerBySube
        [HttpGet]
        public async Task<IActionResult> GetPersonellerBySube(int subeId)
        {
            var personeller = await _context.Personeller
                .Where(p => p.SubeId == subeId && (p.Rol == "Satış Temsilcisi" || p.Rol == "Şube Müdürü"))
                .Select(p => new
                {
                    id = p.PersonelId,
                    display = $"{p.AdSoyad} ({p.Rol})"
                })
                .ToListAsync();

            return Json(personeller);
        }

        // GET: Kiralama/TeslimEt/5
        public async Task<IActionResult> TeslimEt(int id)
        {
            var kiralama = await _context.Kiralamalar
                .Include(k => k.Arac)
                .Include(k => k.Musteri)
                .Include(k => k.CikisSube)
                .Include(k => k.DonusSube)
                .FirstOrDefaultAsync(k => k.KiralamaId == id);

            if (kiralama == null)
            {
                return NotFound();
            }

            if (kiralama.TeslimEdildiMi)
            {
                TempData["ErrorMessage"] = "Bu araç zaten teslim alınmış.";
                return RedirectToAction(nameof(Index));
            }

            kiralama.FiiliTeslimTarihi = DateTime.Today;
            kiralama.TeslimKm = kiralama.AlisKm + 100;

            return View(kiralama);
        }

        // POST: Kiralama/TeslimEt/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeslimEt(int id, int teslimKm, DateTime fiiliTeslimTarihi, decimal hasarBedeli, IFormFile? hasarFotografi)
        {
            var kiralama = await _context.Kiralamalar
                .Include(k => k.Arac)
                .Include(k => k.Musteri)
                .Include(k => k.CikisSube)
                .Include(k => k.DonusSube)
                .FirstOrDefaultAsync(k => k.KiralamaId == id);

            if (kiralama == null)
            {
                return NotFound();
            }

            if (teslimKm < kiralama.AlisKm)
            {
                ModelState.AddModelError("TeslimKm", "Teslim kilometresi, alış kilometresinden düşük olamaz.");
                return View(kiralama);
            }

            // Calculate KM Overage Fee
            decimal kmAsimUcreti = 0;
            int katEdilenYol = teslimKm - kiralama.AlisKm;
            if (katEdilenYol > kiralama.LimitKm)
            {
                int asilanKm = katEdilenYol - kiralama.LimitKm;
                kmAsimUcreti = asilanKm * kiralama.KmAsimUcretiPerKm;
            }

            // Calculate Late Return Fee
            decimal gecikmeUcreti = 0;
            if (fiiliTeslimTarihi > kiralama.BitisTarihi)
            {
                int gecikenGun = (fiiliTeslimTarihi - kiralama.BitisTarihi).Days;
                gecikmeUcreti = gecikenGun * kiralama.GecikmeUcretiPerGun;
            }

            // Handle Photo Upload
            if (hasarFotografi != null && hasarFotografi.Length > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                var fileName = $"hasar_{kiralama.KiralamaId}_{Guid.NewGuid()}{Path.GetExtension(hasarFotografi.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await hasarFotografi.CopyToAsync(stream);
                }
                kiralama.HasarFotografiYolu = $"/uploads/{fileName}";
            }

            kiralama.TeslimKm = teslimKm;
            kiralama.FiiliTeslimTarihi = fiiliTeslimTarihi;
            kiralama.HasarBedeli = hasarBedeli;
            kiralama.TeslimEdildiMi = true;

            kiralama.ToplamTutar += kmAsimUcreti + gecikmeUcreti + hasarBedeli;

            if (kiralama.Arac != null)
            {
                // Drop-off location update logic:
                // If drop-off branch is different from the car's current branch, move the car to the drop-off branch.
                if (kiralama.Arac.BulunduguSubeId != kiralama.DonusSubeId)
                {
                    kiralama.Arac.BulunduguSubeId = kiralama.DonusSubeId;
                }
                
                kiralama.Arac.Durum = "Bos";
                _context.Update(kiralama.Arac);
            }

            _context.Update(kiralama);
            await _context.SaveChangesAsync();

            // Trigger EventBus alert if vehicle damaged
            if (hasarBedeli > 0)
            {
                await CustomEventBus.Instance.PublishAsync(new VehicleDamagedEvent
                {
                    AracId = kiralama.AracId,
                    KiralamaId = kiralama.KiralamaId,
                    HasarBedeli = hasarBedeli,
                    MusteriId = kiralama.MusteriId,
                    Kullanici = "Yonetici Ali"
                });
            }

            TempData["SuccessMessage"] = $"Araç başarıyla teslim alındı. Şube Güncellendi (Aracın bulunduğu yeni şube ID: {kiralama.Arac?.BulunduguSubeId}). Km Aşımı: {kmAsimUcreti:C}, Gecikme Ücreti: {gecikmeUcreti:C}, Hasar: {hasarBedeli:C}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Kiralama/DownloadContract/5
        public async Task<IActionResult> DownloadContract(int id)
        {
            var kiralama = await _context.Kiralamalar.FindAsync(id);
            if (kiralama == null || string.IsNullOrEmpty(kiralama.SozlesmePdfYolu))
            {
                return NotFound("Sözleşme dosyası bulunamadı.");
            }

            var filePath = Path.Combine(_env.WebRootPath, kiralama.SozlesmePdfYolu.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Belge sunucuda bulunamadı.");
            }

            var contentType = kiralama.SozlesmePdfYolu.EndsWith(".pdf") ? "application/pdf" : "text/plain";
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, contentType, Path.GetFileName(filePath));
        }
    }
}
