namespace VyshKat.Models;

public abstract class InventoryItem : IInventoryItem
{
    protected InventoryItem(string name, string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название не должно быть пустым", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(inventoryNumber))
        {
            throw new ArgumentException("Инвентарный номер не должен быть пустым", nameof(inventoryNumber));
        }

        Name = name;
        InventoryNumber = inventoryNumber;
    }

    public string Name { get; }
    public string InventoryNumber { get; }
}
