using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;
using AracKiralamaApp.Models;

namespace AracKiralamaApp.Services
{
    public class MLService
    {
        private readonly AracKiralamaContext _context;

        public MLService(AracKiralamaContext context)
        {
            _context = context;
        }

        // K-Means Clustering for Customer Segmentation
        public async Task RunKMeansSegmentationAsync()
        {
            var customers = await _context.Musteriler
                .Include(m => m.Kiralamalar)
                .ToListAsync();

            if (customers.Count < 3) return; // Need at least 3 customers to form 3 clusters

            // 1. Feature Extraction: (RentalsCount, TotalSpent)
            var dataPoints = customers.Select(c => new
            {
                Customer = c,
                RentalsCount = (double)(c.Kiralamalar?.Count ?? 0),
                TotalSpent = (double)(c.Kiralamalar?.Sum(k => k.ToplamTutar) ?? 0)
            }).ToList();

            // Normalize features to range [0, 1] for fair distance comparison
            double maxRentals = dataPoints.Max(p => p.RentalsCount);
            double maxSpent = dataPoints.Max(p => p.TotalSpent);

            var normalizedPoints = dataPoints.Select(p => new KMeansPoint
            {
                Customer = p.Customer,
                X = maxRentals > 0 ? p.RentalsCount / maxRentals : 0,
                Y = maxSpent > 0 ? p.TotalSpent / maxSpent : 0
            }).ToList();

            // 2. Initialize Centroids (Seed K=3 clusters: VIP, Standard, Pasif)
            // Centroid 0: VIP (Ideally high X, high Y)
            // Centroid 1: Standard (Ideally mid X, mid Y)
            // Centroid 2: Pasif (Ideally low X, low Y)
            var centroids = new List<Centroid>
            {
                new Centroid { X = 1.0, Y = 1.0, Name = "VIP" },
                new Centroid { X = 0.5, Y = 0.5, Name = "Standart" },
                new Centroid { X = 0.0, Y = 0.0, Name = "Pasif" }
            };

            int maxIterations = 10;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                // Assign points to nearest centroid
                foreach (var point in normalizedPoints)
                {
                    double minDist = double.MaxValue;
                    Centroid? bestCentroid = null;

                    foreach (var centroid in centroids)
                    {
                        double dist = Math.Sqrt(Math.Pow(point.X - centroid.X, 2) + Math.Pow(point.Y - centroid.Y, 2));
                        if (dist < minDist)
                        {
                            minDist = dist;
                            bestCentroid = centroid;
                        }
                    }

                    point.AssignedCentroid = bestCentroid;
                }

                // Recalculate centroids
                foreach (var centroid in centroids)
                {
                    var assignedPoints = normalizedPoints.Where(p => p.AssignedCentroid == centroid).ToList();
                    if (assignedPoints.Any())
                    {
                        centroid.X = assignedPoints.Average(p => p.X);
                        centroid.Y = assignedPoints.Average(p => p.Y);
                    }
                }
            }

            // 3. Save cluster segments back to DB
            foreach (var point in normalizedPoints)
            {
                if (point.AssignedCentroid != null)
                {
                    point.Customer.KiralamaSikligiSegment = point.AssignedCentroid.Name;
                }
            }

            await _context.SaveChangesAsync();
        }

        // KNN Pricing Optimizer
        // Recommends daily rate for a vehicle based on year, rental month (seasonality), and duration
        public async Task<decimal> OptimizePriceKNNAsync(int aracId, int targetYear, int targetMonth, int durationDays)
        {
            var targetVehicle = await _context.Araclar.FindAsync(aracId);
            if (targetVehicle == null) return 0;

            // Fetch historical rentals to use as dataset
            var historicalRentals = await _context.Kiralamalar
                .Include(k => k.Arac)
                .Where(k => k.AracId == aracId && k.ToplamTutar > 0)
                .ToListAsync();

            // If we don't have historical data for this specific car, fallback to cars of the same Brand
            if (historicalRentals.Count < 3)
            {
                historicalRentals = await _context.Kiralamalar
                    .Include(k => k.Arac)
                    .Where(k => k.Arac!.Marka == targetVehicle.Marka && k.ToplamTutar > 0)
                    .ToListAsync();
            }

            // If we still have no historical rentals, return base daily price
            if (historicalRentals.Count == 0)
            {
                return targetVehicle.GunlukFiyat;
            }

            // KNN search logic: calculate distance for each history point
            // Features: Car Year, Month (seasonality), Duration (days)
            var neighbors = new List<NeighborDistance>();
            foreach (var rental in historicalRentals)
            {
                int rentalDays = (rental.BitisTarihi - rental.BaslangicTarihi).Days;
                if (rentalDays <= 0) rentalDays = 1;
                decimal historicDailyPrice = rental.ToplamTutar / rentalDays;

                // Feature Distance weights:
                // Car Year: Diff in years (weight 1.0)
                // Seasonality: Min months difference (weight 2.0 because seasonality has high pricing impact)
                // Duration: Diff in rental duration days (weight 0.5)
                double distYear = Math.Abs(targetYear - (rental.Arac?.Yil ?? targetYear));
                
                int monthDiff = Math.Abs(targetMonth - rental.BaslangicTarihi.Month);
                double distMonth = Math.Min(monthDiff, 12 - monthDiff); // circular diff

                double distDuration = Math.Abs(durationDays - rentalDays);

                double totalDistance = Math.Sqrt(
                    Math.Pow(distYear * 1.0, 2) + 
                    Math.Pow(distMonth * 2.0, 2) + 
                    Math.Pow(distDuration * 0.5, 2)
                );

                neighbors.Add(new NeighborDistance
                {
                    Distance = totalDistance,
                    DailyPrice = historicDailyPrice
                });
            }

            // Sort neighbors by distance and take K=3 nearest neighbors
            var nearestK = neighbors.OrderBy(n => n.Distance).Take(3).ToList();
            
            // Return weighted average daily price
            double totalWeight = 0;
            decimal weightedPriceSum = 0;

            foreach (var n in nearestK)
            {
                // Invert distance for weight (avoid division by zero)
                double weight = 1.0 / (n.Distance + 0.1); 
                totalWeight += weight;
                weightedPriceSum += n.DailyPrice * (decimal)weight;
            }

            decimal recommendedPrice = weightedPriceSum / (decimal)totalWeight;
            
            // Bound recommendation within ±30% of original daily price to keep it realistic
            decimal basePrice = targetVehicle.GunlukFiyat;
            decimal minPrice = basePrice * 0.70m;
            decimal maxPrice = basePrice * 1.30m;

            return Math.Round(Math.Clamp(recommendedPrice, minPrice, maxPrice), 2);
        }
    }

    public class KMeansPoint
    {
        public Musteri Customer { get; set; } = null!;
        public double X { get; set; } // Normalized RentalsCount
        public double Y { get; set; } // Normalized TotalSpent
        public Centroid? AssignedCentroid { get; set; }
    }

    public class Centroid
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class NeighborDistance
    {
        public double Distance { get; set; }
        public decimal DailyPrice { get; set; }
    }
}
