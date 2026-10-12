using Xunit;
using Microsoft.Extensions.DependencyInjection;
using VyshKat.Models;
using VyshKat.Services;

namespace VyshKat.Tests;

public class ParkTests
{
    [Fact]
    public void WorkingTransportPassesInspection()
    {
        var park = new Park(new ServiceCenter());

        bool added = park.AddTransport(new Bicycle("Велосипед", "B-1", 7, true));

        Assert.True(added);
        Assert.Equal(1, park.TransportCount);
    }

    [Fact]
    public void BrokenTransportDoesNotEnterPark()
    {
        var park = new Park(new ServiceCenter());

        bool added = park.AddTransport(new Bicycle("Велосипед", "B-1", 7, false));

        Assert.False(added);
        Assert.Empty(park.Items);
    }

    [Fact]
    public void FakeInspectionCanRejectWorkingTransport()
    {
        var fake = new FakeServiceCenter(false);
        var park = new Park(fake);

        bool added = park.AddTransport(new ElectricScooter("Самокат", "S-1", 7, true, 2));

        Assert.False(added);
        Assert.Equal(1, fake.Calls);
        Assert.Equal(0, park.TransportCount);
    }

    [Fact]
    public void FakeInspectionCanAcceptBrokenTransport()
    {
        var fake = new FakeServiceCenter(true);
        var park = new Park(fake);

        bool added = park.AddTransport(new Bicycle("Велосипед", "B-1", 5, false));

        Assert.True(added);
        Assert.Equal(1, fake.Calls);
        Assert.Single(park.Items);
    }

    [Fact]
    public void DependencyInjectionUsesProvidedInspection()
    {
        var fake = new FakeServiceCenter(false);
        var services = new ServiceCollection();
        services.AddSingleton<IServiceCenter>(fake);
        services.AddTransient<Park>();
        using var provider = services.BuildServiceProvider();
        var park = provider.GetRequiredService<Park>();

        bool added = park.AddTransport(new Bicycle("Велосипед", "B-1", 8, true));

        Assert.False(added);
        Assert.Equal(1, fake.Calls);
    }

    [Fact]
    public void SumIncludesElectricTransportAndChargingStations()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new ElectricScooter("Самокат", "S-1", 8, true, 2.5m));
        park.AddTransport(new ElectricBike("Электровелосипед", "E-1", 7, true, 1.2m));
        park.AddTransport(new Bicycle("Велосипед", "B-1", 6, true));
        park.AddItem(new ChargingStation("Станция", "C-1", 0.8m));
        park.AddItem(new Helmet("Шлем", "H-1"));

        Assert.Equal(4.5m, park.TotalDailyEnergy());
        Assert.Equal(3, park.TransportCount);
        Assert.Equal(5, park.InventoryCount);
    }

    [Fact]
    public void RejectedTransportDoesNotConsumeEnergy()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new ElectricScooter("Неисправный", "S-1", 8, false, 3.5m));

        Assert.Equal(0m, park.TotalDailyEnergy());
        Assert.Equal(0, park.InventoryCount);
    }

    [Fact]
    public void BeginnerThresholdIsInclusive()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new Bicycle("Сложный", "B-1", 5, true));
        park.AddTransport(new Bicycle("Подходит", "B-2", 6, true));
        park.AddTransport(new ElectricScooter("Простой", "S-1", 10, true, 1));

        var items = park.ForBeginners();

        Assert.Equal(2, items.Count);
        Assert.Equal("B-2", items[0].InventoryNumber);
        Assert.Equal("S-1", items[1].InventoryNumber);
    }

    [Fact]
    public void BeginnerListDoesNotIncludeRejectedTransport()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new Bicycle("Неисправный", "B-1", 10, false));

        Assert.Empty(park.ForBeginners());
    }

    [Fact]
    public void InventoryContainsTransportAndOtherItems()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new Bicycle("Велосипед", "B-1", 6, true));
        park.AddItem(new Helmet("Шлем", "H-1"));

        Assert.Collection(park.Items,
            item => Assert.Equal("B-1", item.InventoryNumber),
            item => Assert.Equal("H-1", item.InventoryNumber));
    }

    [Fact]
    public void DuplicateInventoryNumberIsForbidden()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new Bicycle("Велосипед", "100", 7, true));

        Assert.Throws<InvalidOperationException>(() => park.AddItem(new Helmet("Шлем", "100")));
        Assert.Equal(1, park.InventoryCount);
    }

    [Fact]
    public void DuplicateTransportNumberIsForbidden()
    {
        var park = new Park(new ServiceCenter());
        park.AddTransport(new Bicycle("Первый", "100", 7, true));

        Assert.Throws<InvalidOperationException>(() =>
            park.AddTransport(new Bicycle("Второй", "100", 7, true)));
    }

    [Fact]
    public void TransportCannotBypassInspectionThroughAddItem()
    {
        var park = new Park(new ServiceCenter());

        Assert.Throws<ArgumentException>(() =>
            park.AddItem(new Bicycle("Велосипед", "B-1", 8, false)));
        Assert.Empty(park.Items);
    }

    [Fact]
    public void InvalidEaseOfUseIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bicycle("Велосипед", "B-1", 0, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bicycle("Велосипед", "B-1", 11, true));
    }

    [Fact]
    public void NegativeEnergyIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ElectricScooter("Самокат", "S-1", 7, true, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargingStation("Станция", "C-1", -1));
    }

    [Fact]
    public void EmptyInventoryNumberIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Helmet("Шлем", ""));
        Assert.Throws<ArgumentException>(() => new Helmet("", "H-1"));
    }

    [Fact]
    public void EmptyParkHasZeroCountsAndEnergy()
    {
        var park = new Park(new ServiceCenter());

        Assert.Equal(0, park.TransportCount);
        Assert.Equal(0, park.InventoryCount);
        Assert.Equal(0m, park.TotalDailyEnergy());
        Assert.Empty(park.ForBeginners());
    }

    private sealed class FakeServiceCenter : IServiceCenter
    {
        private readonly bool _result;

        public FakeServiceCenter(bool result)
        {
            _result = result;
        }

        public int Calls { get; private set; }

        public bool Approve(Transport transport)
        {
            Calls++;
            return _result;
        }
    }
}
