using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AracKiralamaApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    MusteriId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TcNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    EhliyetNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskSkoru = table.Column<int>(type: "int", nullable: false),
                    Blacklist = table.Column<bool>(type: "bit", nullable: false),
                    KiralamaSikligiSegment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.MusteriId);
                });

            migrationBuilder.CreateTable(
                name: "Subeler",
                columns: table => new
                {
                    SubeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubeAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sehir = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AylikKiraGideri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AcilisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subeler", x => x.SubeId);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kullanici = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IslemTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TabloAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EskiVeri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YeniVeri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAdresi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Araclar",
                columns: table => new
                {
                    AracId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plaka = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Marka = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    BulunduguSubeId = table.Column<int>(type: "int", nullable: false),
                    SatinAlmaBedeli = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GunlukFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Araclar", x => x.AracId);
                    table.ForeignKey(
                        name: "FK_Araclar_Subeler_BulunduguSubeId",
                        column: x => x.BulunduguSubeId,
                        principalTable: "Subeler",
                        principalColumn: "SubeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    PersonelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubeId = table.Column<int>(type: "int", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Maas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IseGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.PersonelId);
                    table.ForeignKey(
                        name: "FK_Personeller_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "SubeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AracBakimLoglari",
                columns: table => new
                {
                    BakimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AracId = table.Column<int>(type: "int", nullable: false),
                    BakimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Maliyet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracBakimLoglari", x => x.BakimId);
                    table.ForeignKey(
                        name: "FK_AracBakimLoglari_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "AracId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kiralamalar",
                columns: table => new
                {
                    KiralamaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AracId = table.Column<int>(type: "int", nullable: false),
                    MusteriId = table.Column<int>(type: "int", nullable: false),
                    CikisSubeId = table.Column<int>(type: "int", nullable: false),
                    DonusSubeId = table.Column<int>(type: "int", nullable: false),
                    IsleyenPersonelId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlisKm = table.Column<int>(type: "int", nullable: false),
                    TeslimKm = table.Column<int>(type: "int", nullable: true),
                    LimitKm = table.Column<int>(type: "int", nullable: false),
                    KmAsimUcretiPerKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GecikmeUcretiPerGun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HasarBedeli = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HasarFotografiYolu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SozlesmePdfYolu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TeslimEdildiMi = table.Column<bool>(type: "bit", nullable: false),
                    FiiliTeslimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kiralamalar", x => x.KiralamaId);
                    table.ForeignKey(
                        name: "FK_Kiralamalar_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "AracId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiralamalar_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "MusteriId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiralamalar_Personeller_IsleyenPersonelId",
                        column: x => x.IsleyenPersonelId,
                        principalTable: "Personeller",
                        principalColumn: "PersonelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiralamalar_Subeler_CikisSubeId",
                        column: x => x.CikisSubeId,
                        principalTable: "Subeler",
                        principalColumn: "SubeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiralamalar_Subeler_DonusSubeId",
                        column: x => x.DonusSubeId,
                        principalTable: "Subeler",
                        principalColumn: "SubeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Musteriler",
                columns: new[] { "MusteriId", "AdSoyad", "Blacklist", "EhliyetNo", "Email", "KiralamaSikligiSegment", "RiskSkoru", "TcNo", "Telefon" },
                values: new object[,]
                {
                    { 1, "Ahmet Yılmaz", false, "TR12345", "ahmet@gmail.com", "Standart", 10, "12345678901", "05321112233" },
                    { 2, "Ayşe Demir", false, "TR54321", "ayse@gmail.com", "Standart", 0, "98765432109", "05423334455" }
                });

            migrationBuilder.InsertData(
                table: "Subeler",
                columns: new[] { "SubeId", "AcilisTarihi", "AylikKiraGideri", "Sehir", "SubeAdi" },
                values: new object[,]
                {
                    { 1, new DateTime(2020, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000.00m, "İstanbul", "İstanbul Kadıköy Şubesi" },
                    { 2, new DateTime(2021, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 35000.00m, "Ankara", "Ankara Kızılay Şubesi" }
                });

            migrationBuilder.InsertData(
                table: "Araclar",
                columns: new[] { "AracId", "BulunduguSubeId", "Durum", "GunlukFiyat", "Marka", "Model", "Plaka", "SatinAlmaBedeli", "Yil" },
                values: new object[,]
                {
                    { 1, 1, "Bos", 850.00m, "Fiat", "Egea", "34ABC123", 600000.00m, 2026 },
                    { 2, 1, "Bos", 750.00m, "Renault", "Clio", "06XYZ987", 550000.00m, 2026 },
                    { 3, 2, "Bakimda", 1500.00m, "Volkswagen", "Passat", "35DEF456", 1200000.00m, 2026 }
                });

            migrationBuilder.InsertData(
                table: "Personeller",
                columns: new[] { "PersonelId", "AdSoyad", "IseGirisTarihi", "Maas", "Rol", "SubeId" },
                values: new object[,]
                {
                    { 1, "Ali Yılmaz", new DateTime(2020, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000.00m, "Şube Müdürü", 1 },
                    { 2, "Merve Kaya", new DateTime(2021, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 28000.00m, "Satış Temsilcisi", 1 },
                    { 3, "Can Demir", new DateTime(2022, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 22000.00m, "Temizlik/Bakım", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AracBakimLoglari_AracId",
                table: "AracBakimLoglari",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_BulunduguSubeId",
                table: "Araclar",
                column: "BulunduguSubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_Plaka",
                table: "Araclar",
                column: "Plaka",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kiralamalar_AracId",
                table: "Kiralamalar",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiralamalar_CikisSubeId",
                table: "Kiralamalar",
                column: "CikisSubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiralamalar_DonusSubeId",
                table: "Kiralamalar",
                column: "DonusSubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiralamalar_IsleyenPersonelId",
                table: "Kiralamalar",
                column: "IsleyenPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiralamalar_MusteriId",
                table: "Kiralamalar",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_TcNo",
                table: "Musteriler",
                column: "TcNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_SubeId",
                table: "Personeller",
                column: "SubeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AracBakimLoglari");

            migrationBuilder.DropTable(
                name: "Kiralamalar");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "Araclar");

            migrationBuilder.DropTable(
                name: "Musteriler");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "Subeler");
        }
    }
}
