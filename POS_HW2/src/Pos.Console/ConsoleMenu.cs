using System.Globalization;
using Pos.Application;
using Pos.Domain.Entities;

namespace Pos.Cli;

public sealed class ConsoleMenu
{
    private readonly PosFacade _app;

    public ConsoleMenu(PosFacade app)
    {
        _app = app;
    }

    public void Run()
    {
        while (true)
        {
            System.Console.WriteLine("\n=== ПОС: Преподавательская оценка студентов ===");
            System.Console.WriteLine("1. Список студентов          2. Добавить студента");
            System.Console.WriteLine("3. Изменить имя студента     4. Удалить студента");
            System.Console.WriteLine("5. Список критериев         6. Добавить критерий");
            System.Console.WriteLine("7. Изменить критерий        8. Удалить критерий");
            System.Console.WriteLine("9. Поставить/изменить оценку 10. Показать оценки");
            System.Console.WriteLine("11. Удалить оценку          12. Итог студента");
            System.Console.WriteLine("13. Экспорт CSV             14. Демонстрация");
            System.Console.WriteLine("0. Выход");

            try
            {
                var choice = ReadInt("Команда: ");
                if (choice == 0)
                    return;

                switch (choice)
                {
                    case 1: PrintStudents(); break;
                    case 2: AddStudent(); break;
                    case 3: RenameStudent(); break;
                    case 4: RemoveStudent(); break;
                    case 5: PrintCriteria(); break;
                    case 6: AddCriterion(); break;
                    case 7: UpdateCriterion(); break;
                    case 8: RemoveCriterion(); break;
                    case 9: SetEvaluation(); break;
                    case 10: PrintEvaluations(); break;
                    case 11: RemoveEvaluation(); break;
                    case 12: ShowTotal(); break;
                    case 13: Export(); break;
                    case 14: Demo(); break;
                    default: System.Console.WriteLine("Нет такой команды."); break;
                }
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException
                or IOException or UnauthorizedAccessException)
            {
                System.Console.WriteLine("Ошибка: " + e.Message);
            }
        }
    }

    private void PrintStudents()
    {
        var list = _app.Students;
        if (list.Count == 0)
            System.Console.WriteLine("Студентов пока нет.");
        for (int i = 0; i < list.Count; i++)
            System.Console.WriteLine($"{i + 1}. {list[i].Name}");
    }

    private void PrintCriteria()
    {
        var list = _app.Criteria;
        if (list.Count == 0)
            System.Console.WriteLine("Критериев пока нет.");
        for (int i = 0; i < list.Count; i++)
            System.Console.WriteLine($"{i + 1}. {list[i].Name}: максимум {list[i].MaxScore}, вес {list[i].Weight}");
    }

    private Student PickStudent()
    {
        var list = _app.Students;
        if (list.Count == 0)
            throw new InvalidOperationException("Сначала добавьте студента.");
        PrintStudents();
        int number = ReadNumber("Номер студента: ", list.Count);
        return list[number - 1];
    }

    private Criterion PickCriterion()
    {
        var list = _app.Criteria;
        if (list.Count == 0)
            throw new InvalidOperationException("Сначала добавьте критерий.");
        PrintCriteria();
        int number = ReadNumber("Номер критерия: ", list.Count);
        return list[number - 1];
    }

    private void AddStudent()
    {
        _app.AddStudent(ReadText("Имя студента: "));
        System.Console.WriteLine("Студент добавлен.");
    }

    private void RenameStudent()
    {
        var student = PickStudent();
        _app.RenameStudent(student.Id, ReadText("Новое имя: "));
        System.Console.WriteLine("Имя изменено.");
    }

    private void RemoveStudent()
    {
        var student = PickStudent();
        if (Confirm("Удалить студента и его оценки? (да/нет): "))
        {
            _app.RemoveStudent(student.Id);
            System.Console.WriteLine("Студент удалён.");
        }
    }

    private void AddCriterion()
    {
        var name = ReadText("Название критерия: ");
        var max = ReadDecimal("Максимальный балл: ");
        var weight = ReadDecimal("Вес (например, 2): ");
        _app.AddCriterion(name, max, weight);
        System.Console.WriteLine("Критерий добавлен.");
    }

    private void UpdateCriterion()
    {
        var criterion = PickCriterion();
        var name = ReadText("Новое название: ");
        var max = ReadDecimal("Новый максимум: ");
        var weight = ReadDecimal("Новый вес: ");
        _app.UpdateCriterion(criterion.Id, name, max, weight);
        System.Console.WriteLine("Критерий изменён.");
    }

    private void RemoveCriterion()
    {
        var criterion = PickCriterion();
        if (Confirm("Удалить критерий и выставленные по нему оценки? (да/нет): "))
        {
            _app.RemoveCriterion(criterion.Id);
            System.Console.WriteLine("Критерий удалён.");
        }
    }

    private void SetEvaluation()
    {
        var student = PickStudent();
        var criterion = PickCriterion();
        var score = ReadDecimal($"Балл (0..{criterion.MaxScore}): ");
        System.Console.Write("Комментарий (можно пустой): ");
        var comment = System.Console.ReadLine();
        _app.SetEvaluation(student.Id, criterion.Id, score, comment);
        System.Console.WriteLine("Оценка сохранена.");
    }

    private void PrintEvaluations()
    {
        var student = PickStudent();
        var list = _app.GetEvaluations(student.Id);
        if (list.Count == 0)
            System.Console.WriteLine("Оценок пока нет.");
        foreach (var x in list)
            System.Console.WriteLine($"{x.CriterionName}: {x.Score}/{x.MaxScore} {x.Comment}");
    }

    private void RemoveEvaluation()
    {
        var student = PickStudent();
        var criterion = PickCriterion();
        _app.RemoveEvaluation(student.Id, criterion.Id);
        System.Console.WriteLine("Оценка удалена.");
    }

    private void ShowTotal()
    {
        var student = PickStudent();
        var mode = ReadMode();
        var result = _app.GetTotal(student.Id, mode);
        System.Console.WriteLine($"{result.StudentName}: {result.Percent:F2}% " +
            $"(оценено {result.GradedCount} из {result.CriteriaCount})");
    }

    private void Export()
    {
        var mode = ReadMode();
        var path = Path.Combine("reports", "totals.csv");
        _app.ExportTotals(path, mode);
        System.Console.WriteLine("CSV сохранён: " + Path.GetFullPath(path));
    }

    private void Demo()
    {
        var path = Path.Combine("reports", "demo.csv");
        var totals = _app.RunDemo(path);
        foreach (var total in totals)
            System.Console.WriteLine($"{total.StudentName}: {total.Percent:F2}%");
        System.Console.WriteLine("CSV сохранён: " + Path.GetFullPath(path));
    }

    private static string ReadMode()
    {
        System.Console.WriteLine("1. Все критерии (пропущенные = 0)");
        System.Console.WriteLine("2. Только оценённые критерии");
        return ReadNumber("Режим: ", 2) == 1 ? "all" : "graded";
    }

    private static int ReadNumber(string prompt, int max)
    {
        while (true)
        {
            int value = ReadInt(prompt);
            if (value >= 1 && value <= max)
                return value;
            System.Console.WriteLine($"Введите число от 1 до {max}.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var line = System.Console.ReadLine() ?? throw new EndOfStreamException();
            if (int.TryParse(line, out int value))
                return value;
            System.Console.WriteLine("Введите целое число.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var text = (System.Console.ReadLine() ?? throw new EndOfStreamException())
                .Trim().Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var value))
                return value;
            System.Console.WriteLine("Введите число.");
        }
    }

    private static string ReadText(string prompt)
    {
        System.Console.Write(prompt);
        return System.Console.ReadLine() ?? throw new EndOfStreamException();
    }

    private static bool Confirm(string prompt)
    {
        System.Console.Write(prompt);
        return string.Equals((System.Console.ReadLine() ?? throw new EndOfStreamException()).Trim(), "да",
            StringComparison.OrdinalIgnoreCase);
    }
}
