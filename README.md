# 🚗 Araç Kiralama (Rent-a-Car) ERP Sistemi

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?style=for-the-badge&logo=.net)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=for-the-badge&logo=csharp)
![MSSQL](https://img.shields.io/badge/MSSQL-LocalDB-CC292B?style=for-the-badge&logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

Modern araç kiralama şirketlerinin filo yönetimi, müşteri ilişkileri (CRM), kiralama sözleşmeleri, şube finansı ve bakım süreçlerini uçtan uca yönetmek için geliştirilmiş **Kurumsal Kaynak Planlama (ERP)** web uygulamasıdır.

---

## 📐 Proje Mimarisi ve Teknolojiler

Bu proje, ölçeklenebilir ve modüler bir mimari üzerine **ASP.NET Core 10 MVC** deseni kullanılarak inşa edilmiştir.

* **Framework & Çalışma Zamanı:** .NET 10.0 (ASP.NET Core MVC)
* **Veritabanı ve ORM:** Entity Framework Core 10 (SQL Server Provider), Code-First yaklaşımı
* **Veritabanı Nesneleri:** 
  * `vw_SubeFinansOzeti`: Şube gelir, personel maaşı, kira gideri ve net kâr-zarar hesabı yapan veritabanı görüntüsü (SQL View).
  * `trg_KiralamaInsert`: Yeni kiralama kaydı oluştuğunda araç durumunu otomatik olarak `'Kirada'` yapan veritabanı tetikleyicisi (SQL Trigger).
* **Olay Tabanlı Mimarı (EventBus Pattern):**
  * `CustomEventBus` entegrasyonu ile araç hasar gördüğünde (`VehicleDamagedEvent`) bakım (`MaintenanceHandler`) ve muhasebe (`AccountingHandler`) süreçlerinin eşzamansız tetiklenmesi.
* **Raporlama Entegrasyonu:** `PDFsharp 6.2.4` kütüphanesi ile dinamik kiralama ve finans PDF raporu üretimi.
* **Yapay Zeka / ML Entegrasyonu:** `MLService` ile araçların geçmiş bakım logları ve km verilerine göre arıza/bakım riski tahmini.

---

## 📦 Temel ERP Modülleri

1. 🚘 **Araç & Filo Yönetimi**
   * Plaka, marka, model, günlük fiyat, kilometre ve durum (Müsait, Kirada, Bakımda) takibi.
   * Araç görsel ve teknik özellik yönetimi.

2. 👥 **Müşteri Yönetimi (CRM)**
   * Müşteri profil bilgileri, ehliyet sınıfı/numarası ve kiralama geçmişi.

3. 📝 **Kiralama Operasyonları**
   * Teslim alma ve teslim etme şubeleri seçimi.
   * Günlük bedel ve kiralama süresine göre otomatik toplam tutar hesabı.
   * Araç tesliminde hasar tespiti ve ek ücret hesaplama.

4. 🏢 **Şube & Personel Yönetimi**
   * Şube bazlı lokasyon, kira gideri ve çalışan personel kayıtları.
   * Personel pozisyon ve maaş takibi.

5. 📊 **Finans & İş Zekası (BI)**
   * SQL View destekli şube finans özetleri (Gelir vs Gider analizi).
   * Grafikler ve görselleştirilmiş istatistik panoları (BI Dashboard).

6. 🛠️ **Bakım & Hasar Yönetimi**
   * Olay güdümlü (Event-Driven) bakım günlüğü kaydı ve hasar maliyeti yansıtma.

---

## 🛠️ Kurulum ve Çalıştırma

### Ön Gereksinimler
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) veya üzeri
* MSSQL LocalDB / SQL Server Instance
* Visual Studio 2022+ / VS Code / JetBrains Rider

### Adım Adım Kurulum

1. **Repoyu Klonlayın:**
   ```bash
   git clone https://github.com/KULLANICI_ADI/rent-a-car-erp.git
   cd rent-a-car-erp
   ```

2. **Ayar Dosyasını Oluşturun:**
   Proje ana dizinindeki `appsettings.Example.json` dosyasını `appsettings.json` ismiyle kopyalayın ve veritabanı bağlantı cümlenizi (`ConnectionString`) kendi SQL Server/LocalDB ortamınıza göre düzenleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AracKiralamaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Veritabanını Hazırlayın ve Uygulamayı Başlatın:**
   Proje ilk başlatıldığında EF Core ve `Program.cs` içerisindeki veritabanı mantığı şemayı, view ve trigger nesnelerini otomatik oluşturacaktır.

   Konsoldan çalıştırmak için:
   ```bash
   dotnet run
   ```
   veya Windows ortamında:
   ```cmd
   Calistir.bat
   ```

4. Tarayıcınızda `https://localhost:5001` veya `http://localhost:5000` adresine gidin.

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
