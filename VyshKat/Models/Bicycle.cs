namespace VyshKat.Models;

public sealed class Bicycle : Transport
{
    public Bicycle(string name, string inventoryNumber, int easeOfUse, bool isWorking)
        : base(name, inventoryNumber, easeOfUse, isWorking)
    {
    }
}
