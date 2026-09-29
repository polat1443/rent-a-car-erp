using Microsoft.EntityFrameworkCore;
using AracKiralamaApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext service
builder.Services.AddDbContext<AracKiralamaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AracKiralamaApp.Services.MLService>();

var app = builder.Build();

// Auto-create/migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AracKiralamaContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Create View vw_SubeFinansOzeti
        context.Database.ExecuteSqlRaw(@"
            CREATE VIEW vw_SubeFinansOzeti AS
            SELECT 
                s.SubeId,
                s.SubeAdi,
                s.Sehir,
                s.AylikKiraGideri,
                COALESCE(p.ToplamMaas, 0) AS ToplamPersonelMaasi,
                COALESCE(k.ToplamGelir, 0) AS ToplamKiralamaGeliri,
                (COALESCE(k.ToplamGelir, 0) - (s.AylikKiraGideri + COALESCE(p.ToplamMaas, 0))) AS NetKarZarar
            FROM Subeler s
            LEFT JOIN (
                SELECT SubeId, SUM(Maas) AS ToplamMaas
                FROM Personeller
                GROUP BY SubeId
            ) p ON s.SubeId = p.SubeId
            LEFT JOIN (
                SELECT CikisSubeId, SUM(ToplamTutar + HasarBedeli) AS ToplamGelir
                FROM Kiralamalar
                GROUP BY CikisSubeId
            ) k ON s.SubeId = k.CikisSubeId;
        ");

        // Create Trigger trg_KiralamaInsert
        context.Database.ExecuteSqlRaw(@"
            CREATE TRIGGER trg_KiralamaInsert
            ON Kiralamalar
            AFTER INSERT
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE a
                SET a.Durum = 'Kirada'
                FROM Araclar a
                INNER JOIN inserted i ON a.AracId = i.AracId;
            END;
        ");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken veya başlatılırken bir hata oluştu.");
    }
}

// Subscribe EventBus handlers
var eventBus = AracKiralamaApp.Infrastructure.EventBus.CustomEventBus.Instance;
eventBus.Subscribe<AracKiralamaApp.Infrastructure.EventBus.VehicleDamagedEvent>(
    new AracKiralamaApp.Infrastructure.EventBus.MaintenanceHandler(app.Services));
eventBus.Subscribe<AracKiralamaApp.Infrastructure.EventBus.VehicleDamagedEvent>(
    new AracKiralamaApp.Infrastructure.EventBus.AccountingHandler(app.Services));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
