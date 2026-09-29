using System;
using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class AracBakimLog
    {
        [Key]
        public int BakimId { get; set; }

        [Required(ErrorMessage = "Araç seçimi zorunludur.")]
        [Display(Name = "Araç")]
        public int AracId { get; set; }
        public Arac? Arac { get; set; }

        [Required(ErrorMessage = "Bakım tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Bakım Tarihi")]
        public DateTime BakimTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Maliyet alanı zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Maliyet 0 veya daha büyük olmalıdır.")]
        [Display(Name = "Maliyet (TL)")]
        public decimal Maliyet { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
    }
}
