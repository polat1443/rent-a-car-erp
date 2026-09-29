using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class SystemLog
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Log Tarihi")]
        public DateTime LogTarihi { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        [Display(Name = "İşlemi Yapan")]
        public string Kullanici { get; set; } = "Sistem";

        [Required]
        [StringLength(50)]
        [Display(Name = "İşlem Türü")]
        public string IslemTuru { get; set; } = string.Empty; // Create, Update, Delete

        [Required]
        [StringLength(100)]
        [Display(Name = "Tablo Adı")]
        public string TabloAdi { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Eski Değerler (Şifreli/JSON)")]
        public string EskiVeri { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Yeni Değerler (Şifreli/JSON)")]
        public string YeniVeri { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "IP Adresi")]
        public string IpAdresi { get; set; } = string.Empty;
    }
}
