using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class Musteri
    {
        [Key]
        public int MusteriId { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC No tam olarak 11 hane olmalıdır.")]
        [Display(Name = "TC No")]
        public string TcNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ehliyet No alanı zorunludur.")]
        [StringLength(20, ErrorMessage = "Ehliyet No en fazla 20 karakter olabilir.")]
        [Display(Name = "Ehliyet No")]
        public string EhliyetNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçersiz telefon numarası formatı.")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi formatı.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Range(0, 100)]
        [Display(Name = "Risk Skoru")]
        public int RiskSkoru { get; set; } = 0; // 0 (Güvenli) - 100 (Çok Riskli)

        [Display(Name = "Kara Listede mi?")]
        public bool Blacklist { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Müşteri Segmenti")]
        public string KiralamaSikligiSegment { get; set; } = "Standart";

        // Navigations
        public ICollection<Kiralama>? Kiralamalar { get; set; }
    }
}
