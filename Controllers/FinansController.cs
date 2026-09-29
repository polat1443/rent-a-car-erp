using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AracKiralamaApp.Controllers
{
    public class FinansController : Controller
    {
        private readonly AracKiralamaContext _context;

        public FinansController(AracKiralamaContext context)
        {
            _context = context;
        }

        // GET: Finans
        public async Task<IActionResult> Index()
        {
            var subeler = await _context.Subeler
                .Include(s => s.Personeller)
                .Include(s => s.Araclar)
                .ToListAsync();

            var kiralamalar = await _context.Kiralamalar.ToListAsync();

            var subeFinansList = new List<SubeFinansalDurumViewModel>();

            foreach (var sube in subeler)
            {
                // Gelir: Şubeden yapılan kiralamaların ToplamTutar ve HasarBedeli toplamı
                decimal gelir = kiralamalar
                    .Where(k => k.CikisSubeId == sube.SubeId)
                    .Sum(k => k.ToplamTutar + k.HasarBedeli);

                // Gider: Şubenin aylık kira gideri + çalışan personel maaşları toplamı
                decimal personelMaaslari = sube.Personeller?.Sum(p => p.Maas) ?? 0;
                decimal gider = sube.AylikKiraGideri + personelMaaslari;

                subeFinansList.Add(new SubeFinansalDurumViewModel
                {
                    SubeId = sube.SubeId,
                    SubeAdi = sube.SubeAdi,
                    ToplamGelir = gelir,
                    ToplamGider = gider
                });
            }

            return View(subeFinansList);
        }
    }

    public class SubeFinansalDurumViewModel
    {
        public int SubeId { get; set; }
        public string SubeAdi { get; set; } = string.Empty;
        public decimal ToplamGelir { get; set; }
        public decimal ToplamGider { get; set; }
        public decimal NetKarZarar => ToplamGelir - ToplamGider;
        public bool KardaMi => NetKarZarar >= 0;
    }
}
