using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AracKiralamaApp.Data
{
    public class AracKiralamaContext : DbContext
    {
        public AracKiralamaContext(DbContextOptions<AracKiralamaContext> options) : base(options)
        {
        }

        public DbSet<Sube> Subeler { get; set; } = null!;
        public DbSet<Personel> Personeller { get; set; } = null!;
        public DbSet<Arac> Araclar { get; set; } = null!;
        public DbSet<AracBakimLog> AracBakimLoglari { get; set; } = null!;
        public DbSet<Musteri> Musteriler { get; set; } = null!;
        public DbSet<Kiralama> Kiralamalar { get; set; } = null!;
        public DbSet<SystemLog> SystemLogs { get; set; } = null!;
        public DbSet<SubeFinansOzeti> SubeFinansOzetleri { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. Birincil Anahtarlar (Primary Keys) ---
            modelBuilder.Entity<Sube>().HasKey(s => s.SubeId);
            modelBuilder.Entity<Personel>().HasKey(p => p.PersonelId);
            modelBuilder.Entity<Arac>().HasKey(a => a.AracId);
            modelBuilder.Entity<AracBakimLog>().HasKey(ab => ab.BakimId);
            modelBuilder.Entity<Musteri>().HasKey(m => m.MusteriId);
            modelBuilder.Entity<Kiralama>().HasKey(k => k.KiralamaId);
            modelBuilder.Entity<Kiralama>().ToTable(tb => tb.HasTrigger("trg_Kiralamalar"));
            modelBuilder.Entity<SystemLog>().HasKey(sl => sl.Id);

            // --- 2. Benzersiz Kısıtlamalar (Unique Constraints) ---
            modelBuilder.Entity<Arac>()
                .HasIndex(a => a.Plaka)
                .IsUnique();

            modelBuilder.Entity<Musteri>()
                .HasIndex(m => m.TcNo)
                .IsUnique();

            // --- 3. İlişkiler (Fluent API) ve Delete Behaviors ---

            // Şube & Personel (1-to-Many, Restrict)
            modelBuilder.Entity<Personel>()
                .HasOne(p => p.Sube)
                .WithMany(s => s.Personeller)
                .HasForeignKey(p => p.SubeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Şube & Araç (1-to-Many, Restrict)
            modelBuilder.Entity<Arac>()
                .HasOne(a => a.BulunduguSube)
                .WithMany(s => s.Araclar)
                .HasForeignKey(a => a.BulunduguSubeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Araç & AraçBakimLog (1-to-Many, Cascade)
            modelBuilder.Entity<AracBakimLog>()
                .HasOne(ab => ab.Arac)
                .WithMany(a => a.BakimLoglari)
                .HasForeignKey(ab => ab.AracId)
                .OnDelete(DeleteBehavior.Cascade);

            // Kiralama & Araç (Restrict)
            modelBuilder.Entity<Kiralama>()
                .HasOne(k => k.Arac)
                .WithMany(a => a.Kiralamalar)
                .HasForeignKey(k => k.AracId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiralama & Müşteri (Restrict)
            modelBuilder.Entity<Kiralama>()
                .HasOne(k => k.Musteri)
                .WithMany(m => m.Kiralamalar)
                .HasForeignKey(k => k.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiralama & Çıkış Şubesi (Restrict)
            modelBuilder.Entity<Kiralama>()
                .HasOne(k => k.CikisSube)
                .WithMany(s => s.CikisKiralamalar)
                .HasForeignKey(k => k.CikisSubeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiralama & Dönüş Şubesi (Restrict)
            modelBuilder.Entity<Kiralama>()
                .HasOne(k => k.DonusSube)
                .WithMany(s => s.DonusKiralamalar)
                .HasForeignKey(k => k.DonusSubeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kiralama & İşleyen Personel (Restrict)
            modelBuilder.Entity<Kiralama>()
                .HasOne(k => k.IsleyenPersonel)
                .WithMany(p => p.Kiralamalar)
                .HasForeignKey(k => k.IsleyenPersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 4. Hassas Veri Tipleri (Decimal Precision) ---
            modelBuilder.Entity<Sube>().Property(s => s.AylikKiraGideri).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Personel>().Property(p => p.Maas).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Arac>().Property(a => a.SatinAlmaBedeli).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Arac>().Property(a => a.GunlukFiyat).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AracBakimLog>().Property(ab => ab.Maliyet).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Kiralama>().Property(k => k.ToplamTutar).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Kiralama>().Property(k => k.HasarBedeli).HasColumnType("decimal(18,2)");

            // --- 5. Seed Data (Örnek Başlangıç Verileri) ---
            modelBuilder.Entity<Sube>().HasData(
                new Sube { SubeId = 1, SubeAdi = "İstanbul Kadıköy Şubesi", Sehir = "İstanbul", AylikKiraGideri = 45000.00m, AcilisTarihi = new DateTime(2020, 5, 10) },
                new Sube { SubeId = 2, SubeAdi = "Ankara Kızılay Şubesi", Sehir = "Ankara", AylikKiraGideri = 35000.00m, AcilisTarihi = new DateTime(2021, 6, 15) }
            );

            modelBuilder.Entity<Personel>().HasData(
                new Personel { PersonelId = 1, AdSoyad = "Ali Yılmaz", Rol = "Şube Müdürü", Maas = 45000.00m, SubeId = 1, IseGirisTarihi = new DateTime(2020, 5, 15) },
                new Personel { PersonelId = 2, AdSoyad = "Merve Kaya", Rol = "Satış Temsilcisi", Maas = 28000.00m, SubeId = 1, IseGirisTarihi = new DateTime(2021, 10, 1) },
                new Personel { PersonelId = 3, AdSoyad = "Can Demir", Rol = "Temizlik/Bakım", Maas = 22000.00m, SubeId = 2, IseGirisTarihi = new DateTime(2022, 2, 20) }
            );

            modelBuilder.Entity<Arac>().HasData(
                new Arac { AracId = 1, Plaka = "34ABC123", Marka = "Fiat", Model = "Egea", GunlukFiyat = 850.00m, Durum = "Bos", SatinAlmaBedeli = 600000.00m, BulunduguSubeId = 1 },
                new Arac { AracId = 2, Plaka = "06XYZ987", Marka = "Renault", Model = "Clio", GunlukFiyat = 750.00m, Durum = "Bos", SatinAlmaBedeli = 550000.00m, BulunduguSubeId = 1 },
                new Arac { AracId = 3, Plaka = "35DEF456", Marka = "Volkswagen", Model = "Passat", GunlukFiyat = 1500.00m, Durum = "Bakimda", SatinAlmaBedeli = 1200000.00m, BulunduguSubeId = 2 }
            );

            modelBuilder.Entity<Musteri>().HasData(
                new Musteri { MusteriId = 1, AdSoyad = "Ahmet Yılmaz", TcNo = "12345678901", Telefon = "05321112233", RiskSkoru = 10, Email = "ahmet@gmail.com", EhliyetNo = "TR12345" },
                new Musteri { MusteriId = 2, AdSoyad = "Ayşe Demir", TcNo = "98765432109", Telefon = "05423334455", RiskSkoru = 0, Email = "ayse@gmail.com", EhliyetNo = "TR54321" }
            );

            modelBuilder.Entity<SubeFinansOzeti>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_SubeFinansOzeti");
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        private void OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var logs = new List<SystemLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is SystemLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var log = new SystemLog
                {
                    LogTarihi = DateTime.Now,
                    Kullanici = "Yonetici Ali",
                    IslemTuru = entry.State.ToString(),
                    TabloAdi = entry.Entity.GetType().Name,
                    IpAdresi = "127.0.0.1",
                    EskiVeri = string.Empty,
                    YeniVeri = string.Empty
                };

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                var oldJson = System.Text.Json.JsonSerializer.Serialize(oldValues);
                var newJson = System.Text.Json.JsonSerializer.Serialize(newValues);

                log.EskiVeri = AracKiralamaApp.Infrastructure.Helpers.EncryptionHelper.Encrypt(oldJson);
                log.YeniVeri = AracKiralamaApp.Infrastructure.Helpers.EncryptionHelper.Encrypt(newJson);

                logs.Add(log);
            }

            foreach (var log in logs)
            {
                SystemLogs.Add(log);
            }
        }
    }
}
