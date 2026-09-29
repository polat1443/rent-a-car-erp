using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class Sube
    {
        [Key]
        public int SubeId { get; set; }

        [Required(ErrorMessage = "Şube adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Şube adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Şube Adı")]
        public string SubeAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir zorunludur.")]
        [StringLength(50, ErrorMessage = "Şehir adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Şehir")]
        public string Sehir { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aylık kira gideri zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Aylık kira gideri 0 veya daha büyük olmalıdır.")]
        [Display(Name = "Aylık Kira Gideri (TL)")]
        public decimal AylikKiraGideri { get; set; }

        [Required(ErrorMessage = "Açılış tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Açılış Tarihi")]
        public DateTime AcilisTarihi { get; set; } = DateTime.Today;

        // Navigations
        public ICollection<Personel>? Personeller { get; set; }
        public ICollection<Arac>? Araclar { get; set; }
        public ICollection<Kiralama>? CikisKiralamalar { get; set; }
        public ICollection<Kiralama>? DonusKiralamalar { get; set; }
    }
}
