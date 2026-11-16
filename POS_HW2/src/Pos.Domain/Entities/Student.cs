namespace Pos.Domain.Entities;

public sealed class Student
{
    public Guid Id { get; }
    public string Name { get; private set; }

    public Student(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Неверный идентификатор студента.");

        Id = id;
        Name = "";
        Rename(name);
    }

    internal void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя студента не может быть пустым.");

        Name = name.Trim();
    }
}
