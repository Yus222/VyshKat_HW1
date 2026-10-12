namespace VyshKat.Models;

public sealed class ElectricBike : ElectricTransport
{
    public ElectricBike(
        string name, string inventoryNumber, int easeOfUse, bool isWorking,
        decimal dailyEnergyConsumption)
        : base(name, inventoryNumber, easeOfUse, isWorking, dailyEnergyConsumption)
    {
    }
}
