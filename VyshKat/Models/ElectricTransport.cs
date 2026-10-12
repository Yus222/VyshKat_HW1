namespace VyshKat.Models;

public abstract class ElectricTransport : Transport, IEnergyConsumer
{
    protected ElectricTransport(
        string name, string inventoryNumber, int easeOfUse, bool isWorking,
        decimal dailyEnergyConsumption)
        : base(name, inventoryNumber, easeOfUse, isWorking)
    {
        if (dailyEnergyConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyConsumption));
        }

        DailyEnergyConsumption = dailyEnergyConsumption;
    }

    public decimal DailyEnergyConsumption { get; }
}
