namespace Pos.Domain.ValueObjects;

public readonly record struct Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Балл не может быть отрицательным.");

        Value = value;
    }
}
