using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class Personel
    {
        [Key]
        public int PersonelId { get; set; }

        [Required(ErrorMessage = "Şube seçimi zorunludur.")]
        [Display(Name = "Çalıştığı Şube")]
        public int SubeId { get; set; }
        public Sube? Sube { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol alanı zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Rol / Görev")]
        public string Rol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maaş alanı zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Maaş 0 veya daha büyük olmalıdır.")]
        [Display(Name = "Maaş (TL)")]
        public decimal Maas { get; set; }

        [Required(ErrorMessage = "İşe giriş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "İşe Giriş Tarihi")]
        public DateTime IseGirisTarihi { get; set; } = DateTime.Today;

        // Navigations
        public ICollection<Kiralama>? Kiralamalar { get; set; }
    }
}
