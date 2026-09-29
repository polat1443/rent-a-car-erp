using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AracKiralamaApp.Models;
using AracKiralamaApp.Data;

namespace AracKiralamaApp.Infrastructure.EventBus
{
    public class VehicleDamagedEvent : IEvent
    {
        public int AracId { get; set; }
        public int KiralamaId { get; set; }
        public decimal HasarBedeli { get; set; }
        public int MusteriId { get; set; }
        public string Kullanici { get; set; } = "Sistem";
    }

    public class MaintenanceHandler : IIntegrationEventHandler<VehicleDamagedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public MaintenanceHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(VehicleDamagedEvent @event)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AracKiralamaContext>();

            var vehicle = await context.Araclar.FindAsync(@event.AracId);
            if (vehicle != null)
            {
                vehicle.Durum = "Bakimda";
                await context.SaveChangesAsync();
            }
        }
    }

    public class AccountingHandler : IIntegrationEventHandler<VehicleDamagedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountingHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Handle(VehicleDamagedEvent @event)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AracKiralamaContext>();

            var customer = await context.Musteriler.FindAsync(@event.MusteriId);
            if (customer != null)
            {
                // Hasar durumunda müşterinin risk skorunu artır
                customer.RiskSkoru = Math.Min(100, customer.RiskSkoru + 30);
                
                // Risk skoru belirli bir eşiği aşarsa kara listeye al
                if (customer.RiskSkoru >= 70)
                {
                    customer.Blacklist = true;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
