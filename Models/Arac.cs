using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AracKiralamaApp.Models
{
    public class Arac
    {
        [Key]
        public int AracId { get; set; }

        [Required(ErrorMessage = "Plaka alanı zorunludur.")]
        [StringLength(20, ErrorMessage = "Plaka en fazla 20 karakter olabilir.")]
        [Display(Name = "Plaka")]
        public string Plaka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marka alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Marka en fazla 50 karakter olabilir.")]
        [Display(Name = "Marka")]
        public string Marka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Model en fazla 50 karakter olabilir.")]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yıl alanı zorunludur.")]
        [Range(1900, 2100, ErrorMessage = "Geçersiz bir yıl giriniz.")]
        [Display(Name = "Yıl")]
        public int Yil { get; set; } = DateTime.Today.Year;

        [Required(ErrorMessage = "Araç şubesi zorunludur.")]
        [Display(Name = "Bulunduğu Şube")]
        public int BulunduguSubeId { get; set; }
        
        [Display(Name = "Bulunduğu Şube")]
        public Sube? BulunduguSube { get; set; }

        [Required(ErrorMessage = "Satın alma bedeli zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Satın alma bedeli 0'dan büyük olmalıdır.")]
        [Display(Name = "Satın Alma Bedeli (TL)")]
        public decimal SatinAlmaBedeli { get; set; }

        [Required(ErrorMessage = "Günlük Fiyat alanı zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Günlük fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Günlük Fiyat (TL)")]
        public decimal GunlukFiyat { get; set; }

        [Required(ErrorMessage = "Durum alanı zorunludur.")]
        [StringLength(20)]
        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Bos"; // "Bos", "Kirada", "Bakimda"

        [Display(Name = "ROI (%)")]
        public decimal RoiYuzdesi
        {
            get
            {
                if (SatinAlmaBedeli <= 0) return 0;
                decimal toplamKazanc = Kiralamalar?.Sum(k => k.ToplamTutar) ?? 0;
                return Math.Round((toplamKazanc / SatinAlmaBedeli) * 100, 2);
            }
        }

        // Navigations
        public ICollection<Kiralama>? Kiralamalar { get; set; }
        public ICollection<AracBakimLog>? BakimLoglari { get; set; }
    }
}
