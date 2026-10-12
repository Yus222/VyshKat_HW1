namespace VyshKat.Models;

public sealed class ElectricScooter : ElectricTransport
{
    public ElectricScooter(
        string name, string inventoryNumber, int easeOfUse, bool isWorking,
        decimal dailyEnergyConsumption)
        : base(name, inventoryNumber, easeOfUse, isWorking, dailyEnergyConsumption)
    {
    }
}
