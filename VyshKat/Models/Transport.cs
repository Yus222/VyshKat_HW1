namespace VyshKat.Models;

public abstract class Transport : InventoryItem
{
    protected Transport(string name, string inventoryNumber, int easeOfUse, bool isWorking)
        : base(name, inventoryNumber)
    {
        if (easeOfUse is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(easeOfUse));
        }

        EaseOfUse = easeOfUse;
        IsWorking = isWorking;
    }

    public int EaseOfUse { get; }
    public bool IsWorking { get; }
}
