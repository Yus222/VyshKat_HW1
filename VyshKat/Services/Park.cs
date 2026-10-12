using VyshKat.Models;

namespace VyshKat.Services;

public sealed class Park
{
    private readonly IServiceCenter _serviceCenter;
    private readonly List<IInventoryItem> _items = new();

    public Park(IServiceCenter serviceCenter)
    {
        _serviceCenter = serviceCenter;
    }

    public int TransportCount => _items.OfType<Transport>().Count();
    public int InventoryCount => _items.Count;
    public IReadOnlyList<IInventoryItem> Items => _items.AsReadOnly();

    public bool AddTransport(Transport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        CheckInventoryNumber(transport.InventoryNumber);

        if (!_serviceCenter.Approve(transport))
        {
            return false;
        }

        _items.Add(transport);
        return true;
    }

    public void AddItem(IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item is Transport)
        {
            throw new ArgumentException("Транспорт добавляется только после техосмотра", nameof(item));
        }

        CheckInventoryNumber(item.InventoryNumber);
        _items.Add(item);
    }

    public decimal TotalDailyEnergy()
    {
        return _items.OfType<IEnergyConsumer>().Sum(item => item.DailyEnergyConsumption);
    }

    public IReadOnlyList<Transport> ForBeginners()
    {
        return _items.OfType<Transport>().Where(item => item.EaseOfUse >= 6).ToList();
    }

    private void CheckInventoryNumber(string inventoryNumber)
    {
        if (_items.Any(item => item.InventoryNumber == inventoryNumber))
        {
            throw new InvalidOperationException("Этот инвентарный номер уже используется");
        }
    }
}
