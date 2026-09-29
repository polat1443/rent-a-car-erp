using System.ComponentModel.DataAnnotations.Schema;

namespace AracKiralamaApp.Models
{
    public class SubeFinansOzeti
    {
        public int SubeId { get; set; }
        public string SubeAdi { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public decimal AylikKiraGideri { get; set; }
        public decimal ToplamPersonelMaasi { get; set; }
        public decimal ToplamKiralamaGeliri { get; set; }
        public decimal NetKarZarar { get; set; }
    }
}
