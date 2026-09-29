using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AracKiralamaApp.Models;
using AracKiralamaApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AracKiralamaApp.Controllers;

public class HomeController : Controller
{
    private readonly AracKiralamaContext _context;

    public HomeController(AracKiralamaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalArac = await _context.Araclar.CountAsync();
        ViewBag.TotalMusteri = await _context.Musteriler.CountAsync();
        ViewBag.ActiveKiralamalar = await _context.Kiralamalar.CountAsync();
        
        ViewBag.BosAracCount = await _context.Araclar.CountAsync(a => a.Durum == "Bos");
        ViewBag.KiradaAracCount = await _context.Araclar.CountAsync(a => a.Durum == "Kirada");
        ViewBag.BakimdaAracCount = await _context.Araclar.CountAsync(a => a.Durum == "Bakimda");

        // Finance calculations
        decimal toplamGelir = await _context.Kiralamalar.SumAsync(k => k.ToplamTutar + k.HasarBedeli);
        decimal toplamSubeKira = await _context.Subeler.SumAsync(s => s.AylikKiraGideri);
        decimal toplamPersonelMaas = await _context.Personeller.SumAsync(p => p.Maas);
        decimal toplamAracOpex = await _context.AracBakimLoglari.SumAsync(ab => ab.Maliyet);
        decimal toplamGider = toplamSubeKira + toplamPersonelMaas + toplamAracOpex;

        ViewBag.ToplamGelir = toplamGelir;
        ViewBag.ToplamGider = toplamGider;

        // Pass latest 5 rentals to display in dashboard
        var sonKiralamalar = await _context.Kiralamalar
            .Include(k => k.Arac)
            .Include(k => k.Musteri)
            .OrderByDescending(k => k.KiralamaId)
            .Take(5)
            .ToListAsync();

        return View(sonKiralamalar);
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardChartData()
    {
        // 1. Son 12 ayın kiralama gelirlerini al
        var startDate = DateTime.Today.AddMonths(-11);
        startDate = new DateTime(startDate.Year, startDate.Month, 1);

        var monthlyData = await _context.Kiralamalar
            .Where(k => k.BaslangicTarihi >= startDate)
            .ToListAsync();

        var months = new List<string>();
        var revenues = new List<decimal>();

        for (int i = 0; i < 12; i++)
        {
            var targetMonthDate = startDate.AddMonths(i);
            var targetMonthName = targetMonthDate.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
            
            decimal sum = monthlyData
                .Where(k => k.BaslangicTarihi.Year == targetMonthDate.Year && k.BaslangicTarihi.Month == targetMonthDate.Month)
                .Sum(k => k.ToplamTutar);

            months.Add(targetMonthName);
            revenues.Add(sum);
        }

        // 2. Filonun anlık durum sayılarını al
        var statusCounts = new[]
        {
            await _context.Araclar.CountAsync(a => a.Durum == "Bos"),
            await _context.Araclar.CountAsync(a => a.Durum == "Kirada"),
            await _context.Araclar.CountAsync(a => a.Durum == "Bakimda")
        };

        return Json(new
        {
            months = months,
            revenues = revenues,
            statusCounts = statusCounts
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetBranchChartData()
    {
        var branches = await _context.Subeler
            .Include(s => s.CikisKiralamalar)
            .Include(s => s.Personeller)
            .ToListAsync();

        var branchData = branches.Select(s => new
        {
            subeAdi = s.SubeAdi,
            gelir = s.CikisKiralamalar?.Sum(k => k.ToplamTutar) ?? 0,
            gider = s.AylikKiraGideri + (s.Personeller?.Sum(p => p.Maas) ?? 0)
        }).ToList();

        return Json(branchData);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
