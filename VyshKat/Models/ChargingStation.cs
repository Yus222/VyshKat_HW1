namespace VyshKat.Models;

public sealed class ChargingStation : InventoryItem, IEnergyConsumer
{
    public ChargingStation(string name, string inventoryNumber, decimal dailyEnergyConsumption)
        : base(name, inventoryNumber)
    {
        if (dailyEnergyConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyConsumption));
        }

        DailyEnergyConsumption = dailyEnergyConsumption;
    }

    public decimal DailyEnergyConsumption { get; }
}
