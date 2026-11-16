namespace Pos.Domain.Entities;

public sealed class Criterion
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public decimal MaxScore { get; private set; }
    public decimal Weight { get; private set; }

    public Criterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Неверный идентификатор критерия.");

        Id = id;
        Name = "";
        Update(name, maxScore, weight);
    }

    internal void Update(string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название критерия не может быть пустым.");
        if (maxScore <= 0)
            throw new ArgumentException("Максимальный балл должен быть больше нуля.");
        if (weight <= 0)
            throw new ArgumentException("Вес должен быть больше нуля.");

        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }
}
