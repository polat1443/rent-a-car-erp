using System;
using System.ComponentModel.DataAnnotations;

namespace AracKiralamaApp.Models
{
    public class Kiralama
    {
        [Key]
        public int KiralamaId { get; set; }

        [Required(ErrorMessage = "Araç seçimi zorunludur.")]
        [Display(Name = "Araç")]
        public int AracId { get; set; }
        public Arac? Arac { get; set; }

        [Required(ErrorMessage = "Müşteri seçimi zorunludur.")]
        [Display(Name = "Müşteri")]
        public int MusteriId { get; set; }
        public Musteri? Musteri { get; set; }

        [Required(ErrorMessage = "Alış şubesi seçimi zorunludur.")]
        [Display(Name = "Alış Şubesi")]
        public int CikisSubeId { get; set; }
        public Sube? CikisSube { get; set; }

        [Required(ErrorMessage = "Teslim şubesi seçimi zorunludur.")]
        [Display(Name = "Teslim Şubesi")]
        public int DonusSubeId { get; set; }
        public Sube? DonusSube { get; set; }

        [Required(ErrorMessage = "Kiralama yapan personel zorunludur.")]
        [Display(Name = "Kiralama Yapan Personel")]
        public int IsleyenPersonelId { get; set; }
        public Personel? IsleyenPersonel { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Planlanan Bitiş Tarihi")]
        public DateTime BitisTarihi { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Toplam Tutar (TL)")]
        public decimal ToplamTutar { get; set; }

        [Required(ErrorMessage = "Alış Kilometresi zorunludur.")]
        [Display(Name = "Alış Kilometresi")]
        public int AlisKm { get; set; }

        [Display(Name = "Teslim Kilometresi")]
        public int? TeslimKm { get; set; }

        [Required(ErrorMessage = "Kilometre limiti zorunludur.")]
        [Display(Name = "Kilometre Limiti")]
        public int LimitKm { get; set; } = 300;

        [Required]
        [Display(Name = "Limit Aşım Km Başı Ücret (TL)")]
        public decimal KmAsimUcretiPerKm { get; set; } = 5.00m;

        [Required]
        [Display(Name = "Günlük Gecikme Ücreti (TL)")]
        public decimal GecikmeUcretiPerGun { get; set; } = 500.00m;

        [Display(Name = "Hasar Bedeli (TL)")]
        public decimal HasarBedeli { get; set; } = 0.00m;

        [StringLength(250)]
        [Display(Name = "Hasar Fotoğrafı Kanıtı")]
        public string? HasarFotografiYolu { get; set; }

        [StringLength(250)]
        [Display(Name = "Kiralama Sözleşmesi PDF")]
        public string? SozlesmePdfYolu { get; set; }

        [Display(Name = "Teslim Edildi mi?")]
        public bool TeslimEdildiMi { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Fiili Teslim Tarihi")]
        public DateTime? FiiliTeslimTarihi { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Aktif"; // "Aktif", "Tamamlandi", "Iptal"
    }
}
