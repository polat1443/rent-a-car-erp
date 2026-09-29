using System;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using AracKiralamaApp.Models;

namespace AracKiralamaApp.Infrastructure.Helpers
{
    public static class PdfHelper
    {
        public static string GenerateContractPdf(Kiralama kiralama, string webRootPath)
        {
            var fileName = $"sozlesme_{kiralama.KiralamaId}.pdf";
            var directoryPath = Path.Combine(webRootPath, "contracts");
            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, fileName);

            try
            {
                using var document = new PdfDocument();
                document.Info.Title = $"Kiralama Sözleşmesi - No: {kiralama.KiralamaId}";

                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                // Define standard fonts. Use standard family names that are universally available on Windows
                var fontTitle = new XFont("Arial", 18);
                var fontSection = new XFont("Arial", 13);
                var fontText = new XFont("Arial", 10);
                var fontBold = new XFont("Arial", 10);

                // Background/header decoration
                gfx.DrawRectangle(XBrushes.DarkSlateBlue, 40, 40, page.Width - 80, 40);
                
                // Title text
                gfx.DrawString("RENT A CAR YÖNETİM PANELİ - KİRALAMA SÖZLEŞMESİ", fontSection, XBrushes.White, new XPoint(60, 65));

                // Metadata
                gfx.DrawString($"Sözleşme No: {kiralama.KiralamaId}", fontBold, XBrushes.Black, new XPoint(45, 110));
                gfx.DrawString($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}", fontText, XBrushes.Black, new XPoint(page.Width - 180, 110));

                gfx.DrawLine(XPens.Gray, 40, 120, page.Width - 40, 120);

                // Customer Info Section
                gfx.DrawString("MÜŞTERİ BİLGİLERİ", fontSection, XBrushes.DarkSlateBlue, new XPoint(45, 145));
                gfx.DrawString($"Ad Soyad: {kiralama.Musteri?.AdSoyad}", fontText, XBrushes.Black, new XPoint(55, 165));
                gfx.DrawString($"TC No: {kiralama.Musteri?.TcNo}", fontText, XBrushes.Black, new XPoint(55, 180));
                gfx.DrawString($"Ehliyet No: {kiralama.Musteri?.EhliyetNo}", fontText, XBrushes.Black, new XPoint(55, 195));
                gfx.DrawString($"Telefon: {kiralama.Musteri?.Telefon}", fontText, XBrushes.Black, new XPoint(page.Width / 2, 165));
                gfx.DrawString($"E-posta: {kiralama.Musteri?.Email}", fontText, XBrushes.Black, new XPoint(page.Width / 2, 180));

                gfx.DrawLine(XPens.LightGray, 40, 230, page.Width - 40, 230);

                // Vehicle Info Section
                gfx.DrawString("ARAÇ BİLGİLERİ", fontSection, XBrushes.DarkSlateBlue, new XPoint(45, 255));
                gfx.DrawString($"Plaka: {kiralama.Arac?.Plaka}", fontText, XBrushes.Black, new XPoint(55, 275));
                gfx.DrawString($"Marka & Model: {kiralama.Arac?.Marka} {kiralama.Arac?.Model} ({kiralama.Arac?.Yil})", fontText, XBrushes.Black, new XPoint(55, 290));
                gfx.DrawString($"Alış Şubesi: {kiralama.CikisSube?.SubeAdi ?? "Merkez"}", fontText, XBrushes.Black, new XPoint(page.Width / 2, 275));
                gfx.DrawString($"Teslim Şubesi: {kiralama.DonusSube?.SubeAdi ?? "Merkez"}", fontText, XBrushes.Black, new XPoint(page.Width / 2, 290));

                gfx.DrawLine(XPens.LightGray, 40, 310, page.Width - 40, 310);

                // Rental Terms Section
                gfx.DrawString("KİRALAMA KOŞULLARI", fontSection, XBrushes.DarkSlateBlue, new XPoint(45, 335));
                gfx.DrawString($"Başlangıç Tarihi: {kiralama.BaslangicTarihi:dd.MM.yyyy}", fontText, XBrushes.Black, new XPoint(55, 355));
                gfx.DrawString($"Planlanan Bitiş: {kiralama.BitisTarihi:dd.MM.yyyy}", fontText, XBrushes.Black, new XPoint(55, 370));
                gfx.DrawString($"Alış Kilometresi: {kiralama.AlisKm} KM", fontText, XBrushes.Black, new XPoint(page.Width / 2, 355));
                gfx.DrawString($"Kilometre Sınırı: {kiralama.LimitKm} KM", fontText, XBrushes.Black, new XPoint(page.Width / 2, 370));

                gfx.DrawLine(XPens.Gray, 40, 400, page.Width - 40, 400);

                // Pricing Summary
                gfx.DrawString("FİNANSAL DETAYLAR", fontSection, XBrushes.DarkSlateBlue, new XPoint(45, 425));
                gfx.DrawString($"Sözleşme Toplam Tutarı:", fontBold, XBrushes.Black, new XPoint(55, 450));
                gfx.DrawString($"{kiralama.ToplamTutar:C2}", fontTitle, XBrushes.DarkRed, new XPoint(55, 475));

                // Signatures area
                gfx.DrawString("Kiralayan Firma Yetkilisi", fontBold, XBrushes.Black, new XPoint(60, 600));
                gfx.DrawLine(XPens.Black, 60, 650, 200, 650);

                gfx.DrawString("Müşteri İmzası", fontBold, XBrushes.Black, new XPoint(page.Width - 200, 600));
                gfx.DrawLine(XPens.Black, page.Width - 200, 650, page.Width - 60, 650);

                // Footer warning
                gfx.DrawString("* Bu belge dijital ortamda otomatik olarak oluşturulmuştur.", fontText, XBrushes.Gray, new XPoint(45, page.Height - 50));

                document.Save(filePath);
            }
            catch (Exception ex)
            {
                // Robust Fallback: Write text-based contract document if PDF creation fails due to system font library bugs
                var txtFileName = $"sozlesme_{kiralama.KiralamaId}.txt";
                var txtFilePath = Path.Combine(directoryPath, txtFileName);
                using (var sw = new StreamWriter(txtFilePath))
                {
                    sw.WriteLine($"KİRALAMA SÖZLEŞMESİ - No: {kiralama.KiralamaId}");
                    sw.WriteLine($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sw.WriteLine($"Müşteri: {kiralama.Musteri?.AdSoyad} (TC: {kiralama.Musteri?.TcNo})");
                    sw.WriteLine($"Araç: {kiralama.Arac?.Plaka} ({kiralama.Arac?.Marka} {kiralama.Arac?.Model})");
                    sw.WriteLine($"Tutar: {kiralama.ToplamTutar:C2}");
                    sw.WriteLine("PDF Kütüphanesi hatası nedeniyle bu geçici sözleşme belgesi oluşturulmuştur.");
                    sw.WriteLine($"Hata Detayı: {ex.Message}");
                }
                return $"/contracts/{txtFileName}";
            }

            return $"/contracts/{fileName}";
        }
    }
}
